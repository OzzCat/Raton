using Microsoft.EntityFrameworkCore;
using Raton.Models.DbModels;
using System.Collections.Generic;
using System.Linq;

namespace Raton.Services.DbServices
{
    public class SeriesService : BaseService<SeriesModel>, ISeriesService
    {
        public SeriesService(DataContext db) : base(db)
        {
        }

        public SeriesModel? GetByID(string ID)
        {
            return db.Series
                       .Where(x => x.ID == ID)
                       .FirstOrDefault();
        }

        public List<SeriesModel> GetAllWithChildren()
        {
            return db.Series.Include(x => x.Catches).ToList();
        }

        public int DeleteAllWhereCatchEmpty()
        {
            db.Series.Where(x => x.Catches.Count == 0).ExecuteDelete();
            return db.SaveChanges();
        }
    }
}
