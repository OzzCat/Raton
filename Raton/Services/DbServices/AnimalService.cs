using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using Raton.Models.DbModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Raton.Services.DbServices
{
    public class AnimalService : BaseService<AnimalModel>, IAnimalService
    {
        public AnimalService(DataContext db) : base(db)
        {
        }

        public AnimalModel? GetByID(string animalID)
        {
            return db.Animals
                       .Where(x => x.ID == animalID)
                       .FirstOrDefault();
        }

        public List<AnimalModel> GetAllWithCatchesAndCatchSeries()
        {
            return db.Animals.Include(x => x.Catches).ThenInclude(x => x.Series).ToList();
        }

        public int DeleteAllWhereCatchesEmpty()
        {
            db.Animals.Where(x => x.Catches.Count == 0).ExecuteDelete();
            return db.SaveChanges();
        }
    }
}
