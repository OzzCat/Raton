using Microsoft.EntityFrameworkCore;
using Raton.Models.DbModels.ServiceInterfaces;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raton.Models.DbModels
{
    [Table("SeriesTable")]
    [Index(nameof(ID), IsUnique = true)]
    public class SeriesModel : ICatchMasterModel, IIteratedModel, IHasStringID
    {
        [Key]
        public int TableID { get; set; }
        public string ID { get; set; }
        public byte ColorA { get; set; }
        public byte ColorR { get; set; }
        public byte ColorG { get; set; }
        public byte ColorB { get; set; }
        public string? Comment { get; set; }

        public List<CatchModel> Catches { get; set; } = new();

        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is not SeriesModel)
            {
                return false;
            }
            return this.ID == ((SeriesModel)obj).ID;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
    }
}
