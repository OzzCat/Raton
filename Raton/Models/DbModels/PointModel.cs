using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Features;
using Raton.Models.DbModels.ServiceInterfaces;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raton.Models.DbModels
{
    [Table("PointTable")]
    [Index(nameof(ID), IsUnique = true)]
    public class PointModel : ICatchMasterModel, IIteratedModel, IHasStringID
    {
        [Key]
        public int TableID { get; set; }
        public string ID { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Comment { get; set; }

        public List<CatchModel> Catches { get; set; } = new();

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is not PointModel)
            {
                return false;
            }
            return this.ID == ((PointModel)obj).ID;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
    }
}
