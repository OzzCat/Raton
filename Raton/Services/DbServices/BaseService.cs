using Microsoft.EntityFrameworkCore;
using Raton.Models.DbModels.ServiceInterfaces;
using System.Collections.Generic;
using System.Linq;

namespace Raton.Services.DbServices
{
    public class BaseService<T> where T : class, IIteratedModel
    {
        protected readonly DataContext db;

        public BaseService(DataContext db)
        {
            this.db = db;
        }

        public int SaveChanges()
        {
            var res = db.SaveChanges();
            return res;
        }

        public int Add(T tModel)
        {
            db.Add(tModel);
            var res = db.SaveChanges();
            return res;
        }

        public void AddWithoutSaving(T tModel)
        {
            db.Add(tModel);
        }

        public int AddRange(List<T> tList)
        {
            db.AddRange(tList);
            var res = db.SaveChanges();
            return res;
        }

        public int Update(T tModel)
        {
            db.Update(tModel);
            var res = db.SaveChanges();
            return res;
        }

        public int Remove(T tModel)
        {
            db.Remove(tModel);
            var res = db.SaveChanges();
            return res;
        }

        public int RemoveRangeByPKList(List<int> pks)
        {
            db.Set<T>().Where(t => pks.Contains(t.TableID)).ExecuteDelete();
            var res = db.SaveChanges();
            return res;
        }

        public int RemoveByPK(int pk)
        {
            var ent = db.Set<T>().Find(pk);
            db.Set<T>().Remove(ent);
            var res = db.SaveChanges();
            return res;
        }

        public T? GetByID(int pk)
        {
            return db.Set<T>().Find(pk);
        }

        public List<T> GetAll()
        {
            return db.Set<T>().ToList();
        }
    }
}
