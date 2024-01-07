using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls;
using DynamicData;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using Raton.Services.DbServices;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System;
using Raton.Tables.Templates.ViewModels;
using System.Collections.Generic;
using Raton.Tables.Models;
using DynamicData.Binding;

namespace Raton.Tables.ViewModels
{
    public class CatchTableViewModel : BaseTableViewModel<TableCatchModel>
    {
        #region Collections
        public SourceList<string> _animals { get; private set; }
        private readonly ReadOnlyObservableCollection<string> _animalsList;
        public ReadOnlyObservableCollection<string> AnimalsList => _animalsList;

        public SourceList<string> _points { get; private set; }
        private readonly ReadOnlyObservableCollection<string> _pointsList;
        public ReadOnlyObservableCollection<string> PointsList => _pointsList;

        public SourceList<string> _series { get; private set; }
        private readonly ReadOnlyObservableCollection<string> _seriesList;
        public ReadOnlyObservableCollection<string> SeriesList => _seriesList;
        #endregion

        #region Headers
        CustomHeader AnimalHeader = new CustomHeader("Animal");
        CustomHeader PointHeader = new CustomHeader("Point");
        CustomHeader SeriesHeader = new CustomHeader("Series");
        CustomHeader DateHeader = new CustomHeader("Date");
        CustomHeader CommentHeader = new CustomHeader("Comment");
        #endregion

        #region Filtering
        Func<TableCatchModel, bool> AnimalFilter(string text)
        {
            if (string.IsNullOrEmpty(text)) return cat => true;
            return cat => cat.Animal.ToLower().Contains(text.ToLower());
        }

        Func<TableCatchModel, bool> PointFilter(string text)
        {
            if (string.IsNullOrEmpty(text)) return cat => true;
            return cat => cat.Point.ToLower().Contains(text.ToLower());
        }

        Func<TableCatchModel, bool> SeriesFilter(string text)
        {
            if (string.IsNullOrEmpty(text)) return cat => true;
            return cat => cat.Series.ToLower().Contains(text.ToLower());
        }
        Func<TableCatchModel, bool> DateFilter(string text)
        {
            if (string.IsNullOrEmpty(text)) return cat => true;
#pragma warning disable CS8602 // Разыменование вероятной пустой ссылки.
            return cat => cat.Date is not null && cat.Date.ToString().ToLower().Contains(text.ToLower());
#pragma warning restore CS8602 // Разыменование вероятной пустой ссылки.
        }
        Func<TableCatchModel, bool> CommentFilter(string text)
        {
            if (string.IsNullOrEmpty(text)) return cat => true;
            return cat => !string.IsNullOrEmpty(cat.Comment) && cat.Comment.ToLower().Contains(text.ToLower());
        }
        #endregion

        #region Sorting
        private int DateSortAscending(TableCatchModel? x, TableCatchModel? y)
        {
            if (x is null || y is null)
                return Comparer<TableCatchModel>.Default.Compare(x, y);
            var a = x.Date;
            var b = y.Date;
            return Comparer<DateTime?>.Default.Compare(a, b);
        }

        private int DateSortDescending(TableCatchModel? x, TableCatchModel? y)
        {
            if (x is null || y is null)
                return Comparer<TableCatchModel>.Default.Compare(x, y);
            var a = x.Date;
            var b = y.Date;
            return Comparer<DateTime?>.Default.Compare(b, a);
        }
        #endregion

        #region DB Services
        private readonly IAnimalService _animalService;
        private readonly IPointService _pointService;
        private readonly ISeriesService _seriesService;
        private readonly ICatchService _catchService;
        #endregion

        public CatchTableViewModel(IScreen screen, 
            IAnimalService animalService, 
            IPointService pointService, 
            ICatchService catchService,
            ISeriesService seriesService) : base(screen)
        {
            _animalService = animalService;
            _pointService = pointService;
            _seriesService = seriesService;
            _catchService = catchService;

            _animals = new SourceList<string>();
            _points = new SourceList<string>();
            _series = new SourceList<string>();

            Observable.Start(() => {
                UpdateView();
            }, RxApp.TaskpoolScheduler);

            _animals.Connect()
                .Sort(SortExpressionComparer<string>.Ascending(x => x))
                .Bind(out _animalsList)
                .Subscribe();
            
            _points.Connect()
                .Sort(SortExpressionComparer<string>.Ascending(x => x))
                .Bind(out _pointsList)
                .Subscribe();
            
            _series.Connect()
                .Sort(SortExpressionComparer<string>.Ascending(x => x))
                .Bind(out _seriesList)
                .Subscribe();

            #region Filtering
            var filterAnimalPredicate = this.WhenAnyValue(x => x.AnimalHeader.SearchText)
                          .DistinctUntilChanged()
                          .Select(AnimalFilter);

            var filterPointPredicate = this.WhenAnyValue(x => x.PointHeader.SearchText)
                          .DistinctUntilChanged()
                          .Select(PointFilter);

            var filterSeriesPredicate = this.WhenAnyValue(x => x.SeriesHeader.SearchText)
                          .DistinctUntilChanged()
                          .Select(SeriesFilter);

            var filterDatePredicate = this.WhenAnyValue(x => x.DateHeader.SearchText)
                          .DistinctUntilChanged()
                          .Select(DateFilter);

            var filterCommentPredicate = this.WhenAnyValue(x => x.CommentHeader.SearchText)
                          .DistinctUntilChanged()
                          .Select(CommentFilter);
            #endregion

            _items.Connect()
                .Filter(filterAnimalPredicate)
                .Filter(filterPointPredicate)
                .Filter(filterSeriesPredicate)
                .Filter(filterDatePredicate)
                .Filter(filterCommentPredicate)
                .Bind(out _itemsList)
                .DisposeMany()
                .Subscribe();

            #region IsDirty Checkers
            _items.Connect()
                .WhenPropertyChanged(x => x.Animal, false)
                .DistinctUntilChanged()
                .Subscribe(value =>
                {
                    value.Sender.IsDirty = true;
                });

            _items.Connect()
                .WhenPropertyChanged(x => x.Point, false)
                .DistinctUntilChanged()
                .Subscribe(value =>
                {
                    value.Sender.IsDirty = true;
                });

            _items.Connect()
                .WhenPropertyChanged(x => x.Series, false)
                .DistinctUntilChanged()
                .Subscribe(value =>
                {
                    value.Sender.IsDirty = true;
                });

            _items.Connect()
                .WhenPropertyChanged(x => x.Date, false)
                .DistinctUntilChanged()
                .Subscribe(value =>
                {
                    value.Sender.IsDirty = true;
                });

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
                new TemplateColumn<TableCatchModel>(
                        AnimalHeader,
                        "AnimalCell",
                        null,
                        GridLength.Star)
                );
            ItemsTree.Columns.Insert(
                1,
                new TemplateColumn<TableCatchModel>(
                        PointHeader,
                        "PointCell",
                        null,
                        GridLength.Star)
                );
            ItemsTree.Columns.Insert(
                2,
                new TemplateColumn<TableCatchModel>(
                        SeriesHeader,
                        "SeriesCell",
                        null,
                        GridLength.Star)
                );
            ItemsTree.Columns.Insert(
                3,
                new TemplateColumn<TableCatchModel>(
                        DateHeader,
                        "DateCell",
                        "DateCellEdit",
                        GridLength.Star,
                        new()
                        {
                            CompareAscending = DateSortAscending,
                            CompareDescending = DateSortDescending,
                        }
                        )
                );
            ItemsTree.Columns.Insert(
                4,
                new TextColumn<TableCatchModel, string>(
                        CommentHeader,
                        x => x.Comment,
                        (r, v) => r.Comment = v ?? string.Empty,
                        new GridLength(3, GridUnitType.Star))
                );
            #endregion
        }

        protected override Action<int> SaveItemChanges =>
            async (int tableID) =>
            {
                var cat = _items.Lookup(tableID).Value;
                var dbCatch = _catchService.GetByID(tableID);
                if (dbCatch is null)
                {
                    var catchBox = MessageBoxManager
                            .GetMessageBoxStandard("Error", "Catch was already deleted from the database",
                            ButtonEnum.Ok);

                    await catchBox.ShowWindowAsync();

                    return;
                }

                var dbAnimal = _animalService.GetByID(cat.Animal);
                if (dbAnimal is null)
                {
                    var animalBox = MessageBoxManager
                        .GetMessageBoxStandard("Error", "Animal was already deleted from the database",
                        ButtonEnum.Ok);

                    await animalBox.ShowWindowAsync();

                    return;
                }

                var dbPoint = _pointService.GetByID(cat.Point);
                if (dbPoint is null)
                {
                    var pointBox = MessageBoxManager
                        .GetMessageBoxStandard("Error", "Point was already deleted from the database",
                        ButtonEnum.Ok);

                    await pointBox.ShowWindowAsync();

                    return;
                }

                var dbSeries = _seriesService.GetByID(cat.Series);
                if (dbSeries == null)
                {
                    var seriesBox = MessageBoxManager
                        .GetMessageBoxStandard("Error", "Series was already deleted from the database",
                        ButtonEnum.Ok);

                    await seriesBox.ShowWindowAsync();

                    return;
                }

                dbCatch.AnimalTableID = dbAnimal.TableID;
                dbCatch.PointTableID = dbPoint.TableID;
                dbCatch.SeriesTableID = dbSeries.TableID;
                dbCatch.Date = cat.Date;
                dbCatch.Comment = cat.Comment;

                _catchService.Update(dbCatch);

                cat.IsDirty = false;
            };

        protected override Action<int> DiscardItemChanges =>
            async (int tableID) =>
            {
                var dbCatch = _catchService.GetByID(tableID);
                if (dbCatch == null)
                {
                    var box = MessageBoxManager
                        .GetMessageBoxStandard("Error", "Catch was already deleted from the database",
                        ButtonEnum.Ok);

                    await box.ShowWindowAsync();

                    return;
                }
                _items.AddOrUpdate(new TableCatchModel(dbCatch));
            };

        protected override Action<int> DeleteItem =>
            async (int tableID) =>
            {
                var box = MessageBoxManager
                        .GetMessageBoxStandard("Delete catch",
                        "Are you sure you want to delete this catch?",
                        ButtonEnum.YesNo);

                var deleteConfirmation = await box.ShowWindowAsync();

                if (deleteConfirmation.Equals(ButtonResult.Yes))
                {
                    _catchService.RemoveByPK(tableID);

                    _items.Remove(tableID);
                }
            };

        public override void UpdateView()
        {
            PopulateAnimals();
            PopulatePoints();
            PopulateSeries();

            var catchSource = _catchService.GetAllWithIDs();

            _items.Edit(innerList =>
            {
                innerList.Clear();
                foreach (var cat in catchSource)
                    innerList.AddOrUpdate(new TableCatchModel(cat));
            });
        }

        private void PopulateAnimals()
        {
            var animalSource = _animalService.GetAll();

            _animals.Edit(innerList =>
            {
                innerList.Clear();
                foreach (var animal in animalSource)
                    innerList.Add(animal.ID);
            });
        }

        private void PopulatePoints()
        {
            var pointSource = _pointService.GetAll();

            _points.Edit(innerList =>
            {
                innerList.Clear();
                foreach (var point in pointSource)
                    innerList.Add(point.ID);
            });
        }

        private void PopulateSeries()
        {
            var seriesSource = _seriesService.GetAll();

            _series.Edit(innerList =>
            {
                innerList.Clear();
                foreach (var series in seriesSource)
                    innerList.Add(series.ID);
            });
        }
    }
}
