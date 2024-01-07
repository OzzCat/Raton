using System.ComponentModel.DataAnnotations;

namespace Raton.Models.DbModels.ServiceInterfaces
{
    public interface IIteratedModel
    {
        [Key]
        public int TableID { get; }
    }
}
