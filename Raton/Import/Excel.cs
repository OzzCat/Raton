using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using OfficeOpenXml;
using Raton.Models.DbModels;
using Raton.Services.DbServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Globalization;
using static Raton.Models.DbModels.Enums.SexEnumClass;
using Raton.Models.DbModels.Enums;

namespace Raton.Import
{
    public static class Excel
    {
        public static async Task<string> ImportXlsx(
            string filePath, IAnimalService animalService,
            IPointService pointService, ICatchService catchService, ISeriesService seriesService)
        {
            var newAnimalList = new List<int>();

            var newPointList = new List<int>();

            var newSeriesList = new List<int>();

            var newCatchList = new List<int>();

            var errorsList = new List<string>();

            var rnd = new Random();

            FileInfo fi = new(filePath);

            using (ExcelPackage excelPackage = new ExcelPackage(fi))
            {
                ExcelWorksheet firstWorksheet = excelPackage.Workbook.Worksheets[0];

                if (firstWorksheet == null)
                {
                    return "Empty excel workbook";
                }

                int line = 2;

                bool rowIsNotEmpty = true;

                while (rowIsNotEmpty)
                {
                    int animalTableID = 0;
                    int pointTableID = 0;
                    int seriesTableID = 0;
                    int catchTableID = 0;

                    #region Animal
                    if (firstWorksheet.Cells[line, 1].Value is null)
                    {
                        errorsList.Add("Line " + line.ToString() + ": No animal ID");
                        line++;
                        continue;
                    }

                    var animalID = firstWorksheet.Cells[line, 1].Value.ToString();

                    var getAnimal = animalService.GetByID(animalID);
                    if (getAnimal is null)
                    {
                        var animalModel = new AnimalModel();
                        animalModel.ID = animalID;

                        if (firstWorksheet.Cells[line, 2].Value is not null)
                        {
                            var st = firstWorksheet.Cells[line, 2].Value.ToString();

                            animalModel.Sex = SexEnumClass.ConvertStringToSexEnum(st);
                        }
                        else
                        {
                            animalModel.Sex = SexEnum.NS;
                        }
                                        
                        animalService.Add(animalModel);

                        animalTableID = animalModel.TableID;

                        newAnimalList.Add(animalTableID);
                    }
                    else
                    {
                        animalTableID = getAnimal.TableID;
                    }
                    #endregion

                    #region Point
                    if (firstWorksheet.Cells[line, 4].Value is null)
                    {
                        errorsList.Add("Line " + line.ToString() + ": No Point ID");
                        line++;
                        continue;
                    }

                    var pointID = firstWorksheet.Cells[line, 4].Value.ToString();

                    var getPoint = pointService.GetByID(pointID);
                    if (getPoint is null)
                    {
                        var pointModel = new PointModel();

                        pointModel.ID = pointID;

                        try
                        {
                            pointModel.Latitude = double.Parse
                                (firstWorksheet.Cells[line, 5].Value.ToString(), CultureInfo.InvariantCulture);
                            pointModel.Longitude = double.Parse
                                (firstWorksheet.Cells[line, 6].Value.ToString(), CultureInfo.InvariantCulture);
                        }
                        catch
                        {
                            errorsList.Add("Line " + line.ToString() + " Latitude or Longitude don't match required pattern");
                            line++;
                            continue;
                        }

                        pointService.Add(pointModel);
                        
                        pointTableID = pointModel.TableID;

                        newPointList.Add(pointTableID);
                    }
                    else
                    {
                        pointTableID = getPoint.TableID;
                    }
                    #endregion

                    #region Date and Series
                    var date = DateTime.Now;
                    try
                    {
                        date = (DateTime)firstWorksheet.Cells[line, 7].Value;
                    }
                    catch
                    {
                        errorsList.Add("Line " + line.ToString() + " DateTime for Catch is not presented or is in wrong format");
                        line++;
                        continue;
                    }

                    string seriesID = string.Empty;

                    if (firstWorksheet.Cells[line, 3].Value is null)
                    {
                        seriesID = date.Year.ToString() + " " + date.Month.ToString();
                    }
                    else
                    {
                        seriesID = firstWorksheet.Cells[line, 3].Value.ToString();
                    }

                    var getSeries = seriesService.GetByID(seriesID);
                    if (getSeries == null) 
                    {
                        var seriesModel = new SeriesModel();
                        seriesModel.ID = seriesID;
                        seriesModel.ColorA = 130;
                        seriesModel.ColorR = Convert.ToByte(rnd.Next(0, 255));
                        seriesModel.ColorG = Convert.ToByte(rnd.Next(0, 255));
                        seriesModel.ColorB = Convert.ToByte(rnd.Next(0, 255));

                        seriesService.Add(seriesModel);

                        seriesTableID = seriesModel.TableID;

                        newSeriesList.Add(seriesTableID);
                    }
                    else
                    {
                        seriesTableID = getSeries.TableID;
                    }
                    #endregion

                    #region Catch
                    var catchModel = new CatchModel();

                    catchModel.AnimalTableID = animalTableID;

                    catchModel.PointTableID = pointTableID;

                    catchModel.SeriesTableID = seriesTableID;

                    catchModel.Date = date;

                    var testUnique = catchService.GetByAnimalPointSeriesAndDate(
                        animalTableID, pointTableID, seriesTableID, date);

                    if (testUnique is not null)
                        errorsList.Add("Line " + line.ToString() + " is a duplicate of already existing catch");

                    catchService.Add(catchModel);

                    catchTableID = catchModel.TableID;

                    newCatchList.Add(catchTableID);
                    #endregion

                    line++;
                    rowIsNotEmpty = false;

                    for (int col = 1; col < 8; col++)
                    {
                        if (firstWorksheet.Cells[line, col].Value is not null)
                            rowIsNotEmpty = true;
                    }
                }
            }

            var str = string.Empty;

            if (errorsList.Count != 0)
            {
                str = "Errors occured during data import:\n";
                foreach (var err in errorsList)
                    str += err + '\n';
                str += "\nDo you want to revert import?";

                var box = MessageBoxManager
                .GetMessageBoxStandard("Import Results", str,
                ButtonEnum.YesNo);

                var deleteConfirmation = await box.ShowWindowAsync();

                if (deleteConfirmation.Equals(ButtonResult.Yes))
                {
                    catchService.RemoveRangeByPKList(newCatchList);
                    seriesService.RemoveRangeByPKList(newSeriesList);
                    pointService.RemoveRangeByPKList(newPointList);
                    animalService.RemoveRangeByPKList(newAnimalList);

                    return "Data not changed";
                }

                return "Updated with previously listed errors";
            }
            return "Succesfully updated";
        }
    }
}
