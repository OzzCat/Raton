using Microsoft.EntityFrameworkCore;
using Raton.Models.DbModels;
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

        public List<CatchModel> GetAllWithIDs()
        {
            return db.Catches
                .Include(x => x.Animal)
                .Include(x => x.Series)
                .Include(x => x.Point)
                .ToList();
        }
    }
}
