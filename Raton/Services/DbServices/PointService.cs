using Microsoft.EntityFrameworkCore;
using Raton.Models.DbModels;
using Splat;
using SQLite;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Raton.Services.DbServices
{
    public class PointService : BaseService<PointModel>, IPointService
    {
        public PointService(DataContext db) : base(db)
        {
        }

        public PointModel? GetByID(string ID)
        {
            return db.Points
                       .Where(x => x.ID == ID)
                       .FirstOrDefault();
        }

        public PointModel? GetByIDWithChildrenAndOtherParents(string ID)
        {
            return db.Points
                       .Where(x => x.ID == ID)
                       .Include(x => x.Catches)
                        .ThenInclude(x => x.Animal)
                       .Include(x => x.Catches)
                        .ThenInclude(x => x.Series)
                       .FirstOrDefault();
        }

        public List<PointModel> GetAllWithChildren()
        {
            return db.Points.Include(x => x.Catches).ToList();
        }

        public int DeleteAllWhereCatchesEmpty()
        {
            db.Points.Where(x => x.Catches.Count == 0).ExecuteDelete();
            return db.SaveChanges();
        }
    }
}
