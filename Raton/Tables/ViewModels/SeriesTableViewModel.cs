using Avalonia.Controls;
using DynamicData;
using Raton.Services.DbServices;
using Raton.Tables.Models;
using Raton.Views;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using Raton.Tables.Templates.ViewModels;
using Avalonia.Controls.Models.TreeDataGrid;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using System;
using System.Reactive.Linq;
using System.Reflection.PortableExecutable;

namespace Raton.Tables.ViewModels
{
    public class SeriesTableViewModel : BaseTableViewModel<TableSeriesModel>
    {
        #region Headers
        CustomHeader NameHeader = new CustomHeader("Series Name");
        CustomHeader CommentHeader = new CustomHeader("Comment");
        #endregion

        #region Filtering
        Func<TableSeriesModel, bool> NameFilter(string text)
        {
            if (string.IsNullOrEmpty(text)) return series => true;
            return series => series.ID.ToLower().Contains(text.ToLower());
        }

        Func<TableSeriesModel, bool> CommentFilter(string text)
        {
            if (string.IsNullOrEmpty(text)) return series => true;
            return series => !string.IsNullOrEmpty(series.Comment) && series.Comment.ToLower().Contains(text.ToLower());
        }
        #endregion

        private readonly ISeriesService _seriesService;

        public SeriesTableViewModel(IScreen screen,
            ISeriesService seriesService) : base(screen)
        {
            _seriesService = seriesService;

            Observable.Start(() => {
                UpdateView();
            }, RxApp.TaskpoolScheduler);

            #region Filtering
            var filterNamePredicate = this.WhenAnyValue(x => x.NameHeader.SearchText)
                         .DistinctUntilChanged()
                         .Select(NameFilter);


            var filterCommentPredicate = this.WhenAnyValue(x => x.CommentHeader.SearchText)
                          .DistinctUntilChanged()
                          .Select(CommentFilter);
            #endregion

            _items.Connect()
                .Filter(filterNamePredicate)
                .Filter(filterCommentPredicate)
                .Bind(out _itemsList)
                .DisposeMany()
                .Subscribe();

            #region IsDirty Checkers
            _items.Connect()
                .WhenPropertyChanged(x => x.ID, false)
                .DistinctUntilChanged()
                .Subscribe(value =>
                {
                    value.Sender.IsDirty = true;
                });

            _items.Connect().WhenPropertyChanged(x => x.ItemColor, false)
                .DistinctUntilChanged()
                .Subscribe(value =>
                {
                    value.Sender.IsDirty = true;
                }
                );

            _items.Connect()
                .WhenPropertyChanged(x => x.Comment, false)
                .DistinctUntilChanged()
                .Subscribe(value =>
                {
                    value.Sender.IsDirty = true;
                });
            #endregion

            ItemsTree.Items = _itemsList;

            #region Setup Columns
            ItemsTree.Columns.Insert(
                0,
                new TextColumn<TableSeriesModel, string>(
                        NameHeader,
                        x => x.ID,
                        (r, v) => r.ID = v ?? string.Empty,
                        GridLength.Star)
                );
            ItemsTree.Columns.Insert(
                1,
                new TemplateColumn<TableSeriesModel>(
                        "Color",
                        "ColorCell",
                        null,
                        GridLength.Star
                        )
                );
            ItemsTree.Columns.Insert(
                2,
                new TextColumn<TableSeriesModel, string>(
                        CommentHeader,
                        x => x.Comment,
                        (r, v) => r.Comment = v ?? string.Empty,
                        GridLength.Star)
                );
            #endregion
        }

        protected override Action<int> SaveItemChanges =>
           async (int tableID) =>
           {
               var serie = _items.Lookup(tableID).Value;
               if (string.IsNullOrEmpty(serie.ID))
               {
                   var box = MessageBoxManager
                       .GetMessageBoxStandard("Error", "Series name can't be empty",
                       ButtonEnum.Ok);

                   await box.ShowWindowAsync();

                   return;
               }
               var dbSerie = _seriesService.GetByID(tableID);
               if (dbSerie == null)
               {
                   var box = MessageBoxManager
                       .GetMessageBoxStandard("Error", "Series was already deleted from the database",
                       ButtonEnum.Ok);

                   await box.ShowWindowAsync();

                   return;
               }


               dbSerie.ID = serie.ID;
               dbSerie.ColorA = serie.ItemColor.A;
               dbSerie.ColorR = serie.ItemColor.R;
               dbSerie.ColorG = serie.ItemColor.G;
               dbSerie.ColorB = serie.ItemColor.B;
               dbSerie.Comment = serie.Comment;

               _seriesService.Update(dbSerie);

               serie.IsDirty = false;
           };

        protected override Action<int> DiscardItemChanges =>
           async (int tableID) =>
           {
               var dbSerie = _seriesService.GetByID(tableID);
               if (dbSerie == null)
               {
                   var box = MessageBoxManager
                       .GetMessageBoxStandard("Error", "Animal was already deleted from the database",
                       ButtonEnum.Ok);

                   await box.ShowWindowAsync();

                   return;
               }
               _items.AddOrUpdate(new TableSeriesModel(dbSerie));
           };

        protected override Action<int> DeleteItem =>
           async (int tableID) =>
           {
               var box = MessageBoxManager
                        .GetMessageBoxStandard("Delete series",
                        "Are you sure you want to delete this series?" +
                        "\nAll catches of this series will also be deleted",
                        ButtonEnum.YesNo);

               var deleteConfirmation = await box.ShowWindowAsync();

               if (deleteConfirmation.Equals(ButtonResult.Yes))
               {
                   _seriesService.RemoveByPK(tableID);

                   _items.Remove(tableID);
               }
           };

        public override void UpdateView()
        {
            var seriesSource = _seriesService.GetAll();

            _items.Edit(innerList =>
            {
                innerList.Clear();
                foreach (var series in seriesSource)
                    innerList.AddOrUpdate(new TableSeriesModel(series));
            });
        }
    }
}
