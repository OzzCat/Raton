using Raton.Models.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raton.Services.DbServices
{
    public interface IBaseService<T>
    {
        int SaveChanges();
        List<T> GetAll();
        T? GetByID(int id);
        int Add(T tModel);
        void AddWithoutSaving(T tModel);
        int AddRange(List<T> tModelsList);
        int Remove(T tModel);
        int RemoveByPK(int pk);
        int RemoveRangeByPKList(List<int> pks);
        int Update(T tModel);
    }
}
