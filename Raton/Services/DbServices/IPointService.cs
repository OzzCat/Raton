using Raton.Models.DbModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raton.Services.DbServices
{
    public interface IPointService : IBaseService<PointModel>
    {
        PointModel? GetByID(string name);

        PointModel? GetByIDWithChildrenAndOtherParents(string name);

        List<PointModel> GetAllWithChildren();

        int DeleteAllWhereCatchesEmpty();
    }
}
