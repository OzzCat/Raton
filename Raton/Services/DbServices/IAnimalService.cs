using Raton.Models.DbModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raton.Services.DbServices
{
    public interface IAnimalService : IBaseService<AnimalModel>
    {
        AnimalModel? GetByID(string number);

        public List<AnimalModel> GetAllWithCatchesAndCatchSeries();

        int DeleteAllWhereCatchesEmpty();
    }
}
