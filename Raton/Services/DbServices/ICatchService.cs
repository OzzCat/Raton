using Raton.Models.DbModels;
using System;
using System.Collections.Generic;

namespace Raton.Services.DbServices
{
    public interface ICatchService : IBaseService<CatchModel>
    {
        List<CatchModel> GetCatchesByAnimalAndSeries(string animalNumber, List<string> series);

        CatchModel? GetByAnimalPointSeriesAndDate(int animalID, int pointID, int seriesID, DateTime? dateTime);

        List<CatchModel> GetCatchesByAnimalAndSeriesWithPointAndSeries(string animalNumber, List<string> series);

        List<CatchModel> GetAllWithParents();
    }
}
