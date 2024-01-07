using System.Collections.Generic;

namespace Raton.Models.DbModels.ServiceInterfaces
{
    public interface ICatchMasterModel
    {
        public List<CatchModel> Catches { get; }
    }
}
