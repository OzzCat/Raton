using Raton.Models.DbModels;
using System.Collections.Generic;

namespace Raton.Services.DbServices
{
    public interface ICatchService : IBaseService<CatchModel>
    {
        List<CatchModel> GetCatchesByAnimalAndSeries(string animalNumber, List<string> series);

        List<CatchModel> GetCatchesByAnimalAndSeriesWithPointAndSeries(string animalNumber, List<string> series);

        List<CatchModel> GetAllWithIDs();
    }
}
