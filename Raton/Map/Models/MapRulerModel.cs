using Avalonia.Media;
using Mapsui.Layers;
using Raton.Models.DbModels;
using ReactiveUI;
using System.Collections.Generic;

namespace Raton.Map.Models
{
    public class MapRulerModel : ReactiveObject, IModelWithLayer
    {
        public string Name { get; set; }

        public SolidColorBrush Color { get; set; }
        public Layer? Layer { get; set; }
        public List<PointModel> Points { get; set; }

        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set { this.RaiseAndSetIfChanged(ref _isChecked, value); }
        }

        public MapRulerModel(string name, byte colorA, byte colorR, byte colorG, byte colorB)
        {
            Name = name;
            Color = new SolidColorBrush(Avalonia.Media.Color.FromArgb(colorA, colorR, colorG, colorB));
            _isChecked = true;
            Layer = null;
            Points = new List<PointModel>();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is not MapRulerModel)
            {
                return false;
            }
            return this.Name == ((MapRulerModel)obj).Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
