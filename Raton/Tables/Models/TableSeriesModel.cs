﻿using Avalonia.Media;
using Raton.Models.DbModels;
using ReactiveUI;

namespace Raton.Tables.Models
{
    public class TableSeriesModel : ReactiveObject, ITableModel
    {
        public int TableID { get; set; }
        private string _id;
        public string ID
        {
            get => _id;
            set { this.RaiseAndSetIfChanged(ref _id, value); }
        }
        private Color _itemColor;
        public Color ItemColor
        {
            get => _itemColor;
            set { this.RaiseAndSetIfChanged(ref _itemColor, value); }
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
        public TableSeriesModel(SeriesModel seriesModel) 
        {
            TableID= seriesModel.TableID;
            _id = seriesModel.ID;
            ItemColor = Color.FromArgb(seriesModel.ColorA, seriesModel.ColorR, seriesModel.ColorG, seriesModel.ColorB);
            _comment = seriesModel.Comment ?? string.Empty;
            _isDirty = false;
        }
    }
}
