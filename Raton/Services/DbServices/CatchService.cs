using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using Raton.Models.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Raton.Services.DbServices
{
    public class CatchService : BaseService<CatchModel>, ICatchService
    {
        public CatchService(DataContext db) : base(db)
        {
        }

        public CatchModel? GetByIDWithParents(int pk)
        {
            return db.Catches
                .Include(x => x.Animal)
                .Include(x => x.Series)
                .Include(x => x.Point)
                .FirstOrDefault(x => x.TableID == pk);
        }

        public List<CatchModel> GetCatchesByAnimalAndSeries(string animalID, List<string> series)
        {
            return db.Catches
                      .Where(x => x.Animal.ID == animalID && series.Contains(x.Series.ID)).ToList();
        }

        public List<CatchModel> GetCatchesByAnimalAndSeriesWithPointAndSeries(string animalID, List<string> series)
        {
            return db.Catches
                       .Where(x => x.Animal.ID == animalID && series.Contains(x.Series.ID))
                       .Include(x => x.Point)
                       .Include(x => x.Series)
                       .ToList();
        }

        public CatchModel? GetByAnimalPointSeriesAndDate(int animalID, int pointID, int seriesID, DateTime? dateTime)
        {
            return db.Catches
                       .Where(x => 
                       x.AnimalTableID == animalID
                       && x.PointTableID == pointID
                       && x.SeriesTableID == seriesID
                       && x.Date == dateTime)
                       .FirstOrDefault();
        }

        public List<CatchModel> GetAllWithParents()
        {
            return db.Catches
                .Include(x => x.Animal)
                .Include(x => x.Series)
                .Include(x => x.Point)
                .ToList();
        }
    }
}
