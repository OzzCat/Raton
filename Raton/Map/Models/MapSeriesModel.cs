using Avalonia.Media;
using Raton.Models.DbModels;
using ReactiveUI;

namespace Raton.Map.Models
{
    public class MapSeriesModel : ReactiveObject
    {
        public int TableID { get; private set; }
        public string Name { get; set; }
        public Color ItemColor { get; set; }

        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set { this.RaiseAndSetIfChanged(ref _isChecked, value); }
        }

        public MapSeriesModel(SeriesModel series)
        {
            TableID = series.TableID;
            Name = series.ID;
            ItemColor = Color.FromArgb(series.ColorA, series.ColorR, series.ColorG, series.ColorB);
            _isChecked = true;
        }
    }
}
