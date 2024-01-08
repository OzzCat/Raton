using Raton.Models.DbModels;
using ReactiveUI;
using System.Globalization;

namespace Raton.Tables.Models
{
    public class TablePointModel : ReactiveObject, ITableModel
    {
        public int TableID { get; private set; }
        private string _id;
        public string ID
        {
            get => _id;
            set { this.RaiseAndSetIfChanged(ref _id, value); }
        }
        private string _latitude;
        public string Latitude
        {
            get => _latitude;
            set { this.RaiseAndSetIfChanged(ref _latitude, value); }
        }
        private string _longitude;
        public string Longitude
        {
            get => _longitude;
            set { this.RaiseAndSetIfChanged(ref _longitude, value); }
        }
        private string _comment;
        public string Comment
        {
            get => _comment;
            set { this.RaiseAndSetIfChanged(ref _comment, value); }
        }
        private bool _isDirty;
        public bool IsDirty
        {
            get => _isDirty;
            set { this.RaiseAndSetIfChanged(ref _isDirty, value); }
        }

        public TablePointModel(PointModel pointModel)
        {
            TableID = pointModel.TableID;
            _id = pointModel.ID;
            _latitude = pointModel.Latitude.ToString(CultureInfo.InvariantCulture);
            _longitude = pointModel.Longitude.ToString(CultureInfo.InvariantCulture);
            _comment = pointModel.Comment ?? string.Empty;
            _isDirty = false;
        }

        public TablePointModel()
        {
            TableID = -1;
            _id = string.Empty;
            _latitude = string.Empty;
            _longitude = string.Empty;
            _comment = string.Empty;
            _isDirty = false;
        }
    }
}
