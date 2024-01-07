using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Raton.Models.DbModels.ServiceInterfaces;

namespace Raton.Models.DbModels
{
    [Table("CatchTable")]
    public class CatchModel : IIteratedModel
    {
        [Key]
        public int TableID { get; set; }
        public int AnimalTableID { get; set; }
        public AnimalModel Animal { get; set; }
        public int PointTableID { get; set; }
        public PointModel Point { get; set; }
        public int SeriesTableID { get; set; }
        public SeriesModel Series { get; set; }

        public DateTime? Date { get; set; }

        public string? Comment { get; set; }
    }
}
