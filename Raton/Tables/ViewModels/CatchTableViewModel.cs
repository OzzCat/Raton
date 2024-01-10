using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls;
using DynamicData;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using Raton.Services.DbServices;
using ReactiveUI;
using System.Reactive.Linq;
using System;
using Raton.Tables.Templates.ViewModels;
using System.Collections.Generic;
using Raton.Tables.Models;
using Raton.Models.DbModels;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace Raton.Tables.ViewModels
{
    public class CatchTableViewModel : BaseTableViewModel<TableCatchModel>
    {
        #region Collections
        public List<string> AnimalsList { get; private set; }

        public List<string> PointsList { get; private set; }

        public List<string> SeriesList { get; private set; }
        #endregion

        #region Headers
        CustomHeader AnimalHeader = new("Animal");
        CustomHeader PointHeader = new("Point");
        CustomHeader SeriesHeader = new("Series");
        CustomHeader DateHeader = new("Date and Time");
        CustomHeader CommentHeader = new("Comment");
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
            return cat => cat.Date is not null && (cat.Date.Value.Date.ToString().ToLower().Contains(text.ToLower())
            || cat.Time.Contains(text.ToLower()));
#pragma warning restore CS8602 // Разыменование вероятной пустой ссылки.
        }
        Func<TableCatchModel, bool> CommentFilter(string text)
        {
            if (string.IsNullOrEmpty(text)) return cat => true;
            return cat => !string.IsNullOrEmpty(cat.Comment) && cat.Comment.ToLower().Contains(text.ToLower());
        }
        #endregion

        #region Sorting
        private int AnimalSortAscending(TableCatchModel? x, TableCatchModel? y)
        {
            if (x is null || y is null)
                return Comparer<TableCatchModel>.Default.Compare(x, y);
            var a = x.Animal;
            var b = y.Animal;
            return Comparer<string>.Default.Compare(a, b);
        }

        private int AnimalSortDescending(TableCatchModel? x, TableCatchModel? y)
        {
            if (x is null || y is null)
                return Comparer<TableCatchModel>.Default.Compare(x, y);
            var a = x.Animal;
            var b = y.Animal;
            return Comparer<string>.Default.Compare(b, a);
        }
        private int PointSortAscending(TableCatchModel? x, TableCatchModel? y)
        {
            if (x is null || y is null)
                return Comparer<TableCatchModel>.Default.Compare(x, y);
            var a = x.Point;
            var b = y.Point;
            return Comparer<string>.Default.Compare(a, b);
        }

        private int PointSortDescending(TableCatchModel? x, TableCatchModel? y)
        {
            if (x is null || y is null)
                return Comparer<TableCatchModel>.Default.Compare(x, y);
            var a = x.Point;
            var b = y.Point;
            return Comparer<string>.Default.Compare(b, a);
        }
        private int SeriesSortAscending(TableCatchModel? x, TableCatchModel? y)
        {
            if (x is null || y is null)
                return Comparer<TableCatchModel>.Default.Compare(x, y);
            var a = x.Series;
            var b = y.Series;
            return Comparer<string>.Default.Compare(a, b);
        }

        private int SeriesSortDescending(TableCatchModel? x, TableCatchModel? y)
        {
            if (x is null || y is null)
                return Comparer<TableCatchModel>.Default.Compare(x, y);
            var a = x.Series;
            var b = y.Series;
            return Comparer<string>.Default.Compare(b, a);
        }
        private int DateSortAscending(TableCatchModel? x, TableCatchModel? y)
        {
            if (x is null || y is null)
                return Comparer<TableCatchModel>.Default.Compare(x, y);
            var a = x.Date;
            var b = y.Date;
            var resD = Comparer<DateTime?>.Default.Compare(a, b);
            if (resD != 0)
                return resD;
            else
                return Comparer<string?>.Default.Compare(x.Time, y.Time);
        }

        private int DateSortDescending(TableCatchModel? x, TableCatchModel? y)
        {
            if (x is null || y is null)
                return Comparer<TableCatchModel>.Default.Compare(x, y);
            var a = x.Date;
            var b = y.Date;
            var resD = Comparer<DateTime?>.Default.Compare(b, a);
            if (resD != 0)
                return resD;
            else
                return Comparer<string?>.Default.Compare(y.Time, x.Time);
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

            AnimalsList = new List<string>();
            PointsList = new List<string>();
            SeriesList = new List<string>();

            Observable.Start(() => {
                UpdateView();
            }, RxApp.TaskpoolScheduler);

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
                .WhenPropertyChanged(x => x.Time, false)
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
                        "AnimalCellEdit",
                        GridLength.Star,
                        new()
                        {
                            CompareAscending = AnimalSortAscending,
                            CompareDescending = AnimalSortDescending,
                        })
                );
            ItemsTree.Columns.Insert(
                1,
                new TemplateColumn<TableCatchModel>(
                        PointHeader,
                        "PointCell",
                        "PointCellEdit",
                        new GridLength(2, GridUnitType.Star),
                        new()
                        {
                            CompareAscending = PointSortAscending,
                            CompareDescending = PointSortDescending,
                        })
                );
            ItemsTree.Columns.Insert(
                2,
                new TemplateColumn<TableCatchModel>(
                        SeriesHeader,
                        "SeriesCell",
                        "SeriesCellEdit",
                        GridLength.Star,
                        new()
                        {
                            CompareAscending = SeriesSortAscending,
                            CompareDescending = SeriesSortDescending,
                        })
                );
            ItemsTree.Columns.Insert(
                3,
                new TemplateColumn<TableCatchModel>(
                        DateHeader,
                        "DateCell",
                        "DateChangeCell",
                        new GridLength(2, GridUnitType.Star),
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
                        new GridLength(2, GridUnitType.Star))
                );
            #endregion
        }

        protected override Action<int> SaveItemChanges =>
            async (int tableID) =>
            {
                var cat = _items.Lookup(tableID).Value;

                #region Validate Input
                if (string.IsNullOrEmpty(cat.Animal))
                {
                    var animalBox = MessageBoxManager
                        .GetMessageBoxStandard("Error", "Animal ID can't be empty",
                        ButtonEnum.Ok);

                    await animalBox.ShowWindowAsync();

                    return;
                }

                if (string.IsNullOrEmpty(cat.Point))
                {
                    var pointBox = MessageBoxManager
                        .GetMessageBoxStandard("Error", "Point ID can't be empty",
                        ButtonEnum.Ok);

                    await pointBox.ShowWindowAsync();

                    return;
                }

                if (string.IsNullOrEmpty(cat.Series))
                {
                    var seriesBox = MessageBoxManager
                        .GetMessageBoxStandard("Error", "Seies ID can't be empty",
                        ButtonEnum.Ok);

                    await seriesBox.ShowWindowAsync();

                    return;
                }

                if (cat.Date is null)
                {
                    var seriesBox = MessageBoxManager
                        .GetMessageBoxStandard("Error", "Date can't be empty",
                        ButtonEnum.Ok);

                    await seriesBox.ShowWindowAsync();

                    return;
                }

                if (TimeSpan.TryParse(cat.Time, out TimeSpan ts))
                {
                    var date = cat.Date.Value;
                    cat.Date = new DateTime(date.Year, date.Month, date.Day, ts.Hours, ts.Minutes, ts.Seconds);
                }
                else
                {
                    var seriesBox = MessageBoxManager
                        .GetMessageBoxStandard("Error", "Time format is incorrect",
                        ButtonEnum.Ok);

                    await seriesBox.ShowWindowAsync();

                    return;
                }
                #endregion

                #region Check Catch Existance
                var dbCatch = _catchService.GetByID(tableID);
                if (dbCatch is null)
                {
                    var catchBox = MessageBoxManager
                            .GetMessageBoxStandard("Error", "Catch was already deleted from the database",
                            ButtonEnum.Ok);

                    await catchBox.ShowWindowAsync();

                    return;
                }
                #endregion

                #region Check Animal Existance
                var dbAnimal = _animalService.GetByID(cat.Animal);
                if (dbAnimal is null)
                {
                    var animalBox = MessageBoxManager
                        .GetMessageBoxStandard("Error", "Animal was already deleted from the database",
                        ButtonEnum.Ok);

                    await animalBox.ShowWindowAsync();

                    return;
                }
                #endregion

                #region Check Point Existance
                var dbPoint = _pointService.GetByID(cat.Point);
                if (dbPoint is null)
                {
                    var pointBox = MessageBoxManager
                        .GetMessageBoxStandard("Error", "Point was already deleted from the database",
                        ButtonEnum.Ok);

                    await pointBox.ShowWindowAsync();

                    return;
                }
                #endregion

                #region Check Series Existance
                var dbSeries = _seriesService.GetByID(cat.Series);
                if (dbSeries == null)
                {
                    var seriesBox = MessageBoxManager
                        .GetMessageBoxStandard("Error", "Series was already deleted from the database",
                        ButtonEnum.Ok);

                    await seriesBox.ShowWindowAsync();

                    return;
                }
                #endregion

                #region Check Unique
                var testUnique = _catchService.GetByAnimalPointSeriesAndDate(
                    dbAnimal.TableID, dbPoint.TableID, dbSeries.TableID, cat.Date.Value);

                if (testUnique is not null)
                {
                    var boxUnique = MessageBoxManager
                        .GetMessageBoxStandard("Error", "Catch with the same parameters already exists",
                        ButtonEnum.Ok);

                    await boxUnique.ShowWindowAsync();
                    return;
                }
                #endregion

                dbCatch.AnimalTableID = dbAnimal.TableID;
                dbCatch.PointTableID = dbPoint.TableID;
                dbCatch.SeriesTableID = dbSeries.TableID;
                dbCatch.Date = cat.Date.Value;
                dbCatch.Comment = cat.Comment;

                _catchService.Update(dbCatch);

                cat.IsDirty = false;
            };

        protected override Action<int> DiscardItemChanges =>
            async (int tableID) =>
            {
                var dbCatch = _catchService.GetByIDWithParents(tableID);
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

        protected override Action ShowOrHideAddPanel =>
            () =>
            {
                if (NewItem is null)
                {
                    NewItem = new TableCatchModel();
                }

                IsAddPanelVisible = !IsAddPanelVisible;
            };

        protected override Action<bool> AddItem =>
            async (bool DiscardEditingValues) =>
            {
                #region Validate Input
                if (NewItem is null)
                    throw new ApplicationException();

                if (string.IsNullOrEmpty(NewItem.Animal))
                {
                    var animalBox = MessageBoxManager
                        .GetMessageBoxStandard("Error", "Animal ID can't be empty",
                        ButtonEnum.Ok);

                    await animalBox.ShowWindowAsync();

                    return;
                }

                if (string.IsNullOrEmpty(NewItem.Point))
                {
                    var pointBox = MessageBoxManager
                        .GetMessageBoxStandard("Error", "Point ID can't be empty",
                        ButtonEnum.Ok);

                    await pointBox.ShowWindowAsync();

                    return;
                }

                if (string.IsNullOrEmpty(NewItem.Series))
                {
                    var seriesBox = MessageBoxManager
                        .GetMessageBoxStandard("Error", "Seies ID can't be empty",
                        ButtonEnum.Ok);

                    await seriesBox.ShowWindowAsync();

                    return;
                }

                if (NewItem.Date is null)
                {
                    var seriesBox = MessageBoxManager
                        .GetMessageBoxStandard("Error", "Date can't be empty",
                        ButtonEnum.Ok);

                    await seriesBox.ShowWindowAsync();

                    return;
                }

                if (TimeSpan.TryParse(NewItem.Time, out TimeSpan ts))
                {
                    var date = NewItem.Date.Value;
                    NewItem.Date = new DateTime(date.Year, date.Month, date.Day, ts.Hours, ts.Minutes, ts.Seconds);
                }
                else
                {
                    var seriesBox = MessageBoxManager
                        .GetMessageBoxStandard("Error", "Time format is incorrect",
                        ButtonEnum.Ok);

                    await seriesBox.ShowWindowAsync();

                    return;
                }
                #endregion

                #region Check Animal Existance
                var dbAnimal = _animalService.GetByID(NewItem.Animal);
                if (dbAnimal is null)
                {
                    var animalBox = MessageBoxManager
                        .GetMessageBoxStandard("Error", "Animal was already deleted from the database",
                        ButtonEnum.Ok);

                    await animalBox.ShowWindowAsync();

                    return;
                }
                #endregion

                #region Check Point Existance
                var dbPoint = _pointService.GetByID(NewItem.Point);
                if (dbPoint is null)
                {
                    var pointBox = MessageBoxManager
                        .GetMessageBoxStandard("Error", "Point was already deleted from the database",
                        ButtonEnum.Ok);

                    await pointBox.ShowWindowAsync();

                    return;
                }
                #endregion

                #region Check Series Existance
                var dbSeries = _seriesService.GetByID(NewItem.Series);
                if (dbSeries == null)
                {
                    var seriesBox = MessageBoxManager
                        .GetMessageBoxStandard("Error", "Series was already deleted from the database",
                        ButtonEnum.Ok);

                    await seriesBox.ShowWindowAsync();

                    return;
                }
                #endregion

                #region Check Unique
                var testUnique = _catchService.GetByAnimalPointSeriesAndDate(
                    dbAnimal.TableID, dbPoint.TableID, dbSeries.TableID, NewItem.Date.Value);

                if (testUnique is not null)
                {
                    var boxUnique = MessageBoxManager
                        .GetMessageBoxStandard("Error", "Catch with the same parameters already exists",
                        ButtonEnum.Ok);

                    await boxUnique.ShowWindowAsync();
                    return;
                }
                #endregion

                var dbCatch = new CatchModel
                {
                    AnimalTableID = dbAnimal.TableID,
                    PointTableID = dbPoint.TableID,
                    SeriesTableID = dbSeries.TableID,
                    Date = NewItem.Date.Value,
                    Comment = NewItem.Comment
                };

                _catchService.Add(dbCatch);

                _items.AddOrUpdate(new TableCatchModel(dbCatch));

                if (DiscardEditingValues)
                    NewItem = new TableCatchModel();
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

            var catchSource = _catchService.GetAllWithParents();

            _items.Edit(innerList =>
            {
                innerList.Clear();
                foreach (var cat in catchSource)
                    innerList.AddOrUpdate(new TableCatchModel(cat));
            });

            IsAddPanelVisible = false;
            NewItem = null;
        }

        private void PopulateAnimals()
        {
            var animalSource = _animalService.GetAll();

            AnimalsList.Clear();

            AnimalsList.Add(string.Empty);

            foreach (var animal in animalSource)
                AnimalsList.Add(animal.ID);
        }

        private void PopulatePoints()
        {
            var pointsSource = _pointService.GetAll();

            PointsList.Clear();

            PointsList.Add(string.Empty);

            foreach (var animal in pointsSource)
                PointsList.Add(animal.ID);
        }

        private void PopulateSeries()
        {
            var seriesSource = _seriesService.GetAll();

            SeriesList.Clear();

            SeriesList.Add(string.Empty);

            foreach (var serie in seriesSource)
                SeriesList.Add(serie.ID);
        }

        #region ComboBoxes Population Functions
        private static bool StringContains(string str, string? query)
        {
            if (query == null) return false;
            return str.Contains(query);
        }

        public Func<string, CancellationToken, Task<IEnumerable<object>>> PopulateAnimalsAsync =>
            async (string query, CancellationToken token) =>
            {
                var res = AnimalsList.Where(x => StringContains(x, query)).ToList();

                return res;
            };
        public Func<string, CancellationToken, Task<IEnumerable<object>>> PopulatePointsAsync =>
            async (string query, CancellationToken token) =>
            {
                var res = PointsList.Where(x => StringContains(x, query)).ToList();

                return res;
            };
        public Func<string, CancellationToken, Task<IEnumerable<object>>> PopulateSeriesAsync =>
            async (string query, CancellationToken token) =>
            {
                var res = SeriesList.Where(x => StringContains(x, query)).ToList();

                return res;
            };
        #endregion
    }
}
