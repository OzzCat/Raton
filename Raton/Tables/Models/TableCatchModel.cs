using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using Raton.Models.DbModels;
using ReactiveUI;
using System;

namespace Raton.Tables.Models
{
    public class TableCatchModel : ReactiveObject, ITableModel
    {
        public int TableID { get; set; }
        private string _animal;
        public string Animal
        {
            get => _animal;
            set { this.RaiseAndSetIfChanged(ref _animal, value); }
        }
        private string _point;
        public string Point
        {
            get => _point;
            set { this.RaiseAndSetIfChanged(ref _point, value); }
        }
        private string _series;
        public string Series
        {
            get => _series;
            set { this.RaiseAndSetIfChanged(ref _series, value); }
        }
        private DateTime? _date;
        public DateTime? Date
        {
            get => _date;
            set { this.RaiseAndSetIfChanged(ref _date, value); }
        }
        private string? _time;
        public string? Time
        {
            get => _time;
            set { this.RaiseAndSetIfChanged(ref _time, value); }
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

        public TableCatchModel(CatchModel catchModel) 
        {
            TableID = catchModel.TableID;
            _animal = catchModel.Animal.ID;
            _point = catchModel.Point.ID;
            _series = catchModel.Series.ID;
            _date = catchModel.Date;
            _time = catchModel.Date.TimeOfDay.ToString("c");
            _comment = catchModel.Comment ?? string.Empty;
            _isDirty = false;
        }

        public TableCatchModel()
        {
            TableID = -1;
            _animal = string.Empty;
            _point = string.Empty;
            _series = string.Empty;
            _date = null;
            _time = "00:00:00";
            _comment = string.Empty;
            _isDirty = false;
        }
    }
}
