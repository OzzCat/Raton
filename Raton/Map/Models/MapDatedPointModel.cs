using Mapsui.Styles;
using Raton.Models.DbModels;
using System;

namespace Raton.Map.Models
{
    public class MapDatedPointModel : IComparable<MapDatedPointModel>
    {
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime Date { get; set; }
        public Color Color { get; set; }

        public MapDatedPointModel(PointModel point, SeriesModel series, DateTime? date)
        {
            Name = point.ID;
            Latitude = point.Latitude;
            Longitude = point.Longitude;
            this.Color = new Color();
            this.Color.A = series.ColorA;
            this.Color.R = series.ColorR;
            this.Color.G = series.ColorG;
            this.Color.B = series.ColorB;
            Date = date ?? DateTime.Today;
        }

        public int CompareTo(MapDatedPointModel? other)
        {
            return Date.CompareTo(other.Date);
        }
    }
}
