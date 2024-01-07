using Raton.Models.DbModels.ServiceInterfaces;

namespace Raton.Tables.Models.SelectorModels
{
    public class IndexableSelectorModel<T> where T : class, IIteratedModel, IHasStringID
    {
        public int TableID { get; private set; }
        public string ID { get; set; }

        public IndexableSelectorModel(T tModel)
        {
            TableID = tModel.TableID;
            ID = tModel.ID;
        }
        
    }
}
