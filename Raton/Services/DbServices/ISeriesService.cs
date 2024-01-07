using Raton.Models.DbModels;
using System.Collections.Generic;

namespace Raton.Services.DbServices
{
    public interface ISeriesService : IBaseService<SeriesModel>
    {
        SeriesModel? GetByID(string name);

        List<SeriesModel> GetAllWithChildren();

        int DeleteAllWhereCatchEmpty();
    }
}
