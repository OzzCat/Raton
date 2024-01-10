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
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Models;

namespace Raton.ExportAndImport
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

                int line = 1;

                bool rowIsNotEmpty = true;

                while (rowIsNotEmpty)
                {
                    line++;
                    rowIsNotEmpty = false;

                    for (int col = 1; col < 8; col++)
                    {
                        if (firstWorksheet.Cells[line, col].Value is not null)
                            rowIsNotEmpty = true;
                    }
                    if (!rowIsNotEmpty)
                        break;

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

                    var animalID = firstWorksheet.Cells[line, 1].Value.ToString().Trim();

                    var getAnimal = animalService.GetByID(animalID);
                    if (getAnimal is null)
                    {
                        var animalModel = new AnimalModel();
                        animalModel.ID = animalID;

                        if (firstWorksheet.Cells[line, 2].Value is not null)
                        {
                            var st = firstWorksheet.Cells[line, 2].Value.ToString();

                            animalModel.Sex = ConvertStringToSexEnum(st);
                        }
                        else
                        {
                            animalModel.Sex = SexEnum.NS;
                        }

                        if (firstWorksheet.Cells[line, 8].Value is not null)
                        {
                            animalModel.Comment = firstWorksheet.Cells[line, 8].Value.ToString();
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
                    if (firstWorksheet.Cells[line, 3].Value is null)
                    {
                        errorsList.Add("Line " + line.ToString() + ": No Point ID");
                        line++;
                        continue;
                    }

                    var pointID = firstWorksheet.Cells[line, 3].Value.ToString().Trim();

                    var getPoint = pointService.GetByID(pointID);
                    if (getPoint is null)
                    {
                        var pointModel = new PointModel();

                        pointModel.ID = pointID;

                        if (firstWorksheet.Cells[line, 4].Value is null)
                        {
                            errorsList.Add("Line " + line.ToString() + ": Latitude can't be null");
                            continue;
                        }
                        else if (firstWorksheet.Cells[line, 4].Value is not double)
                        {

                            if (double.TryParse(firstWorksheet.Cells[line, 4].Value.ToString()
                                .Replace(" ", "").Replace(",", "."),
                                    NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                                    CultureInfo.InvariantCulture, out double latitude))
                            {
                                pointModel.Latitude = latitude;
                            }
                            else
                            {
                                errorsList.Add("Line " + line.ToString() + ": Latitude don't match required pattern");
                                continue;
                            }
                        }
                        else
                        {
                            pointModel.Latitude = (double)firstWorksheet.Cells[line, 4].Value;
                        }

                        if (Math.Abs(pointModel.Latitude) > 90)
                        {
                            errorsList.Add("Line " + line.ToString() + ": Latitude is out of bounds");
                            continue;
                        }

                        if (firstWorksheet.Cells[line, 5].Value is null)
                        {
                            errorsList.Add("Line " + line.ToString() + ": Longitude can't be null");
                            continue;
                        }
                        else if (firstWorksheet.Cells[line, 5].Value is not double)
                        {
                            if (double.TryParse(firstWorksheet.Cells[line, 5].Value.ToString()
                                .Replace(" ", "").Replace(",", "."),
                                    NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                                    CultureInfo.InvariantCulture, out double longitude))
                            {
                                pointModel.Longitude = longitude;
                            }
                            else
                            {
                                errorsList.Add("Line " + line.ToString() + " Longitude don't match required pattern");
                                continue;
                            }
                        }
                        else
                        {
                            pointModel.Longitude = (double)firstWorksheet.Cells[line, 5].Value;
                        }

                        if (Math.Abs(pointModel.Longitude) > 180)
                        {
                            errorsList.Add("Line " + line.ToString() + ": Longitude is out of bounds");
                            continue;
                        }

                        if (firstWorksheet.Cells[line, 9].Value is not null)
                        {
                            pointModel.Comment = firstWorksheet.Cells[line, 9].Value.ToString();
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
                    var date = new DateTime();
                    if (firstWorksheet.Cells[line, 7].Value is null)
                    {
                        errorsList.Add("Line " + line.ToString() + ": DateTime for Catch is not presented");
                        continue;
                    }
                    if (firstWorksheet.Cells[line, 7].Value is double)
                    {
                        date = DateTime.FromOADate((double)firstWorksheet.Cells[line, 7].Value);
                    }
                    else if (firstWorksheet.Cells[line, 7].Value is DateTime)
                    {
                        date = (DateTime)firstWorksheet.Cells[line, 7].Value;
                    }
                    // Add parsing from scanner check
                    else if (DateTime.TryParse(firstWorksheet.Cells[line, 7].Value.ToString(), out date))
                    {
                    }
                    else
                    {
                        errorsList.Add("Line " + line.ToString() + ": DateTime for Catch is in the wrong format");
                        continue;
                    }

                    string seriesID = string.Empty;

                    if (firstWorksheet.Cells[line, 6].Value is null)
                    {
                        seriesID = date.Year.ToString() + " " + date.Month.ToString();
                    }
                    else
                    {
                        seriesID = firstWorksheet.Cells[line, 6].Value.ToString().Trim();
                    }

                    var getSeries = seriesService.GetByID(seriesID);
                    if (getSeries == null)
                    {
                        var seriesModel = new SeriesModel();

                        seriesModel.ID = seriesID;

                        if (firstWorksheet.Cells[line, 10].Value is not null)
                        {
                            seriesModel.Comment = firstWorksheet.Cells[line, 10].Value.ToString();
                        }

                        if (firstWorksheet.Cells[line, 12].Value is not null
                            && firstWorksheet.Cells[line, 13].Value is not null
                            && firstWorksheet.Cells[line, 14].Value is not null
                            && firstWorksheet.Cells[line, 15].Value is not null)
                        {
                            try
                            {
                                seriesModel.ColorA = byte.Parse(firstWorksheet.Cells[line, 12].Value.ToString());
                                seriesModel.ColorR = byte.Parse(firstWorksheet.Cells[line, 13].Value.ToString());
                                seriesModel.ColorG = byte.Parse(firstWorksheet.Cells[line, 14].Value.ToString());
                                seriesModel.ColorB = byte.Parse(firstWorksheet.Cells[line, 15].Value.ToString());
                            }
                            catch
                            {
                                seriesModel.ColorA = 255;
                                seriesModel.ColorR = Convert.ToByte(rnd.Next(0, 255));
                                seriesModel.ColorG = Convert.ToByte(rnd.Next(0, 255));
                                seriesModel.ColorB = Convert.ToByte(rnd.Next(0, 255));
                            }
                        }
                        else
                        {
                            seriesModel.ColorA = 130;
                            seriesModel.ColorR = Convert.ToByte(rnd.Next(0, 255));
                            seriesModel.ColorG = Convert.ToByte(rnd.Next(0, 255));
                            seriesModel.ColorB = Convert.ToByte(rnd.Next(0, 255));
                        }

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

                    if (firstWorksheet.Cells[line, 11].Value is not null)
                    {
                        catchModel.Comment = firstWorksheet.Cells[line, 11].Value.ToString();
                    }

                    catchService.Add(catchModel);

                    catchTableID = catchModel.TableID;

                    newCatchList.Add(catchTableID);
                    #endregion
                }
            }

            var str = string.Empty;

            if (errorsList.Count != 0)
            {
                str = "Errors occured during data import\n";
                str += "\nDo you want to revert import?";
                foreach (var err in errorsList)
                    str += err + '\n';
                

                var box = MessageBoxManager
                .GetMessageBoxCustom(
                    new MessageBoxCustomParams
                    {
                        ButtonDefinitions = new List<ButtonDefinition>
                        {
                            new ButtonDefinition { Name = "Yes", },
                            new ButtonDefinition { Name = "No", },
                        },

                        ContentTitle = "title",
                        MaxHeight= 400,
                        ContentMessage = str,
                        ShowInCenter = true
                    });

                

                var deleteConfirmation = await box.ShowWindowAsync();

                if (deleteConfirmation.Equals("Yes"))
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


        public static void ExportToXlsx(
            string filePath, IAnimalService animalService,
            IPointService pointService, ICatchService catchService, ISeriesService seriesService)
        {
            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                excelPackage.Workbook.Worksheets.Add(DateTime.Today.ToString());
                ExcelWorksheet firstWorksheet = excelPackage.Workbook.Worksheets[0];
                #region Create Headers
                firstWorksheet.Cells[1, 1].Value = "Animal";
                firstWorksheet.Cells[1, 2].Value = "Sex";
                firstWorksheet.Cells[1, 3].Value = "Point";
                firstWorksheet.Cells[1, 4].Value = "Latitude";
                firstWorksheet.Cells[1, 5].Value = "Longitude";
                firstWorksheet.Cells[1, 6].Value = "Series";
                firstWorksheet.Cells[1, 7].Value = "Date";
                firstWorksheet.Cells[1, 8].Value = "Animal Comment";
                firstWorksheet.Cells[1, 9].Value = "Point Comment";
                firstWorksheet.Cells[1, 10].Value = "Series Comment";
                firstWorksheet.Cells[1, 11].Value = "Catch Comment";
                firstWorksheet.Cells[1, 12].Value = "Series Color A";
                firstWorksheet.Cells[1, 13].Value = "Series Color R";
                firstWorksheet.Cells[1, 14].Value = "Series Color G";
                firstWorksheet.Cells[1, 15].Value = "Series Color B";
                #endregion

                var catchesList = catchService.GetAllWithParents();

                int line = 2;

                foreach (var cat in catchesList)
                {
                    firstWorksheet.Cells[line, 1].Value = cat.Animal.ID;
                    firstWorksheet.Cells[line, 2].Value = SexEnumClass.ConvertFromSexEnumToString(cat.Animal.Sex);
                    firstWorksheet.Cells[line, 3].Value = cat.Point.ID;
                    firstWorksheet.Cells[line, 4].Value = cat.Point.Latitude;
                    firstWorksheet.Cells[line, 5].Value = cat.Point.Longitude;
                    firstWorksheet.Cells[line, 6].Value = cat.Series.ID;
                    firstWorksheet.Cells[line, 7].Value = cat.Date.ToOADate();
                    firstWorksheet.Cells[line, 8].Value = cat.Animal.Comment;
                    firstWorksheet.Cells[line, 9].Value = cat.Point.Comment;
                    firstWorksheet.Cells[line, 10].Value = cat.Series.Comment;
                    firstWorksheet.Cells[line, 11].Value = cat.Comment;
                    firstWorksheet.Cells[line, 12].Value = cat.Series.ColorA;
                    firstWorksheet.Cells[line, 13].Value = cat.Series.ColorR;
                    firstWorksheet.Cells[line, 14].Value = cat.Series.ColorG;
                    firstWorksheet.Cells[line, 15].Value = cat.Series.ColorB;

                    line++;
                }

                FileInfo fi = new(filePath);

                excelPackage.SaveAs(fi);
            }
        }
    }
}
