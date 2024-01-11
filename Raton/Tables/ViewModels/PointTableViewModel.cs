using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using DynamicData;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using Raton.Services.DbServices;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using Raton.Tables.Models;
using Raton.Tables.Templates.ViewModels;
using System.Globalization;
using Raton.Models.DbModels;

namespace Raton.Tables.ViewModels
{
    public class PointTableViewModel : BaseTableViewModel<TablePointModel>
    {   
        #region Headers
        CustomHeader IDHeader = new CustomHeader("Name");
        CustomHeader LatitudeHeader = new CustomHeader("Latitude");
        CustomHeader LongitudeHeader = new CustomHeader("Longitude");
        CustomHeader CommentHeader = new CustomHeader("Comment");
        #endregion

        #region Filtering
        Func<TablePointModel, bool> nameFilter(string text)
        {
            if (string.IsNullOrEmpty(text)) return point => true;
            return point => point.ID.ToLower().Contains(text.ToLower());
        }

        Func<TablePointModel, bool> latitudeFilter(string text)
        {
            if (string.IsNullOrEmpty(text)) return point => true;
            return point => point.Latitude.ToString().Contains(text);
        }

        Func<TablePointModel, bool> longitudeFilter(string text)
        {
            if (string.IsNullOrEmpty(text)) return point => true;
            return point => point.Longitude.ToString().Contains(text);
        }

        Func<TablePointModel, bool> commentFilter(string text)
        {
            if (string.IsNullOrEmpty(text)) return point => true;
            return point => !string.IsNullOrEmpty(point.Comment) && point.Comment.ToLower().Contains(text.ToLower());
        }
        #endregion

        private readonly IPointService _pointService;

        public PointTableViewModel(IScreen screen, IPointService pointService) : base(screen)
        {
            _pointService = pointService;

            NewItem = new TablePointModel();

            Observable.Start(() => {
                UpdateView();
            }, RxApp.TaskpoolScheduler);

            #region Filtering
            var filterNamePredicate = this.WhenAnyValue(x => x.IDHeader.SearchText)
                          .DistinctUntilChanged()
                          .Select(nameFilter);

            var filterLatitudePredicate = this.WhenAnyValue(x => x.LatitudeHeader.SearchText)
                          .DistinctUntilChanged()
                          .Select(latitudeFilter);

            var filterLongitudePredicate = this.WhenAnyValue(x => x.LongitudeHeader.SearchText)
                          .DistinctUntilChanged()
                          .Select(longitudeFilter);

            var filterCommentPredicate = this.WhenAnyValue(x => x.CommentHeader.SearchText)
                          .DistinctUntilChanged()
                          .Select(commentFilter);
            #endregion

            _items.Connect()
                .Filter(filterNamePredicate)
                .Filter(filterLatitudePredicate)
                .Filter(filterLongitudePredicate)
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

            _items.Connect()
                .WhenPropertyChanged(x => x.Latitude, false)
                .DistinctUntilChanged()
                .Subscribe(value =>
                {
                    value.Sender.IsDirty = true;
                });

            _items.Connect()
                .WhenPropertyChanged(x => x.Longitude, false)
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
                new TextColumn<TablePointModel, string>(
                        IDHeader,
                        x => x.ID,
                        (r, v) => r.ID = v ?? string.Empty,
                        new GridLength(2, GridUnitType.Star))
                );
            ItemsTree.Columns.Insert(
                1,
                new TextColumn<TablePointModel, string>(LatitudeHeader,
                    x => x.Latitude,
                    (r, v) => r.Latitude = v ?? string.Empty,
                    GridLength.Star)
                );
            ItemsTree.Columns.Insert(
                2,
                new TextColumn<TablePointModel, string>(
                        LongitudeHeader,
                        x => x.Longitude,
                        (r, v) => r.Longitude = v ?? string.Empty,
                        GridLength.Star)
                );
            ItemsTree.Columns.Insert(
                3,
                new TextColumn<TablePointModel, string>(
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
                var point = _items.Lookup(tableID).Value;

                #region Validate Input
                if (string.IsNullOrEmpty(point.ID))
                {
                    var box = MessageBoxManager
                        .GetMessageBoxStandard("Error", "Point ID can't be empty",
                        ButtonEnum.Ok);

                    await box.ShowWindowAsync();

                    return;
                }
                double latitude = 0;
                double longitude = 0;
                if (!double.TryParse(point.Latitude.Replace(',', '.'),
                    NumberStyles.AllowDecimalPoint
                    | NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture, out latitude)
                || Math.Abs(latitude) > 90)
                {
                    var boxLat = MessageBoxManager
                        .GetMessageBoxStandard("Error", "Point " + point.ID + " Latitude is in the wrong format",
                        ButtonEnum.Ok);

                    await boxLat.ShowWindowAsync();
                    return;
                }
                if (!double.TryParse(point.Longitude.Replace(',', '.'),
                    NumberStyles.AllowDecimalPoint
                    | NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture, out longitude)
                || Math.Abs(longitude) > 180)
                {
                    var boxLon = MessageBoxManager
                        .GetMessageBoxStandard("Error", "Point " + point.ID + " Longitude is in the wrong format",
                        ButtonEnum.Ok);

                    await boxLon.ShowWindowAsync();
                    return;
                }
                #endregion

                var dbPoint = _pointService.GetByID(tableID);

                #region Check Point Existance
                if (dbPoint == null)
                {
                    var box = MessageBoxManager
                        .GetMessageBoxStandard("Error", "Point was already deleted from the database",
                        ButtonEnum.Ok);

                    await box.ShowWindowAsync();

                    return;
                }
                #endregion

                #region Check Unique
                var testUnique = _pointService.GetByID(point.ID);
                if (testUnique is not null)
                {
                    if (dbPoint.TableID != testUnique.TableID)
                    {
                        var boxUnique = MessageBoxManager
                        .GetMessageBoxStandard("Error", "Point with the same Name already exists",
                        ButtonEnum.Ok);

                        await boxUnique.ShowWindowAsync();
                        return;
                    }   
                }
                #endregion

                dbPoint.ID = point.ID;
                dbPoint.Latitude = latitude;
                dbPoint.Longitude = longitude;
                dbPoint.Comment = point.Comment;

                _pointService.Update(dbPoint);

                point.IsDirty = false;
            };


        protected override Action<int> DiscardItemChanges =>
            async (int tableID) =>
            {
                var dbPoint = _pointService.GetByID(tableID);
                if (dbPoint == null)
                {
                    var box = MessageBoxManager
                        .GetMessageBoxStandard("Error", "Point was already deleted from the database",
                        ButtonEnum.Ok);

                    await box.ShowWindowAsync();

                    return;
                }
                _items.AddOrUpdate(new TablePointModel(dbPoint));
            };

        protected override Action<bool> AddItem =>
            async (bool DiscardEditingValues) =>
            {
                #region Validate Input
                if (NewItem is null)
                    throw new ApplicationException();

                if (string.IsNullOrEmpty(NewItem.ID))
                {
                    var box = MessageBoxManager
                        .GetMessageBoxStandard("Error", "Point ID can't be empty",
                        ButtonEnum.Ok);

                    await box.ShowWindowAsync();

                    return;
                }
                double latitude = 0;
                double longitude = 0;
                if (!double.TryParse(NewItem.Latitude.Replace(',', '.'),
                    NumberStyles.AllowDecimalPoint
                    | NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture, out latitude)
                || latitude > 90 || latitude < -90)
                {
                    var boxLat = MessageBoxManager
                        .GetMessageBoxStandard("Error", "New Point Latitude is in the wrong format",
                        ButtonEnum.Ok);

                    await boxLat.ShowWindowAsync();
                    return;
                }
                if (!double.TryParse(NewItem.Longitude.Replace(',', '.'),
                    NumberStyles.AllowDecimalPoint
                    | NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture, out longitude)
                || longitude > 180 || longitude < -180)
                {
                    var boxLon = MessageBoxManager
                        .GetMessageBoxStandard("Error", "New Point Longitude is in the wrong format",
                        ButtonEnum.Ok);

                    await boxLon.ShowWindowAsync();
                    return;
                }
                #endregion

                #region Check Unique
                var testUnique = _pointService.GetByID(NewItem.ID);
                if (testUnique is not null)
                {
                    var boxUnique = MessageBoxManager
                        .GetMessageBoxStandard("Error", "Point with the same Name already exists",
                        ButtonEnum.Ok);

                    await boxUnique.ShowWindowAsync();
                    return;
                }
                #endregion

                var dbPoint = new PointModel
                {
                    ID = NewItem.ID,
                    Latitude = latitude,
                    Longitude = longitude,
                    Comment = NewItem.Comment
                };

                _pointService.Add(dbPoint);

                _items.AddOrUpdate(new TablePointModel(dbPoint));

                if (DiscardEditingValues)
                    NewItem = new TablePointModel();
            };

        protected override Action<int> DeleteItem =>
            async (int tableID) =>
            {
                var box = MessageBoxManager
                        .GetMessageBoxStandard("Delete point",
                        "Are you sure you want to delete this point?" +
                        "\nAll catches of this point will also be deleted",
                        ButtonEnum.YesNo);

                var deleteConfirmation = await box.ShowWindowAsync();

                if (deleteConfirmation.Equals(ButtonResult.Yes))
                {
                    _pointService.RemoveByPK(tableID);

                    _items.Remove(tableID);
                }
            };

        public override void UpdateView()
        {
            var pointSource = _pointService.GetAll();

            _items.Edit(innerList =>
            {
                innerList.Clear();
                foreach (var point in pointSource)
                    innerList.AddOrUpdate(new TablePointModel(point));
            });
        }
    }
}
