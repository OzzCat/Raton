using Avalonia.Media;
using Raton.Models.DbModels;
using static Raton.Models.DbModels.Enums.SexEnumClass;

namespace Raton.Map.Models
{
    public class CatchPositionModel
    {
        public int AnimalTableID { get; set; }
        public string Animal { get; set; }
        public SexEnum Sex { get; set; }
        public SolidColorBrush AColor { get; set; }
        public string Date { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public CatchPositionModel(AnimalModel animal, PointModel point, CatchModel cat) 
        {
            AnimalTableID = animal.TableID;
            Animal = animal.ID;
            Sex = animal.Sex;
            Latitude = point.Latitude;
            Longitude = point.Longitude;

            Date = cat.Date.ToString() ?? string.Empty;

            AColor = new SolidColorBrush(Color.FromArgb(cat.Series.ColorA, cat.Series.ColorR, cat.Series.ColorG, cat.Series.ColorB));
        }

    }
}
