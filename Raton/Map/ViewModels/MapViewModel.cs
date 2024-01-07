using Avalonia;
using DynamicData;
using DynamicData.Binding;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Nts.Extensions;
using Mapsui.Projections;
using Mapsui.UI.Avalonia;
using Mapsui.Widgets.ScaleBar;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using Raton.Models.DbModels;
using Raton.Services.DbServices;
using Raton.Map.ViewModels.Dialogs;
using Raton.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Raton.Map.Models;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using System.Windows.Input;
using Splat;
using Mapsui.Tiling;
using BruTile.MbTiles;
using Mapsui.Tiling.Layers;

namespace Raton.Map.ViewModels
{
    public class MapViewModel : ReactiveObject, IRoutableViewModel, IUpdatable
    {
        #region Routing
        public string UrlPathSegment { get; } = "map";

        public IScreen HostScreen { get; }
        #endregion

        #region Collections
        private SourceCache<MapAnimalModel, int> _animals;
        private readonly ReadOnlyObservableCollection<MapAnimalModel> _animalsList;
        public ReadOnlyObservableCollection<MapAnimalModel> AnimalsList => _animalsList;

        private SourceList<CatchPositionModel> _selectedPointAnimals;
        private readonly ReadOnlyObservableCollection<CatchPositionModel> _selectedPointAnimalsList;
        public ReadOnlyObservableCollection<CatchPositionModel> SelectedPointAnimalsList => _selectedPointAnimalsList;

        private SourceCache<MapSeriesModel, int> _series;
        private readonly ReadOnlyObservableCollection<MapSeriesModel> _seriesList;
        public ReadOnlyObservableCollection<MapSeriesModel> SeriesList => _seriesList;
        #endregion

        public MapControl CurrentMapControl { get; private set; }

        #region Ruler Variables and Commands

        private SourceCache<MapRulerModel, string> _rulers;
        private readonly ReadOnlyObservableCollection<MapRulerModel> _rulersList;
        public ReadOnlyObservableCollection<MapRulerModel> RulersList => _rulersList;

        MapRulerModel? _selectedRuler;

        public MapRulerModel? SelectedRuler
        {
            get => _selectedRuler;
            set => this.RaiseAndSetIfChanged(ref _selectedRuler, value);
        }

        public ReactiveCommand<int, Unit> AnimalCheckedCommand { get; }

        public ReactiveCommand<Unit, Unit> SeriesCheckedCommand { get; }

        public ReactiveCommand<Unit, Unit> AddRuler { get; }
        public ReactiveCommand<string, Unit> DeleteRuler { get; }
        #endregion

        #region Selection Commands
        public ReactiveCommand<Unit, Unit> SelectAllAnimals { get; }

        public ReactiveCommand<Unit, Unit> SelectAllSeries { get; }

        public ReactiveCommand<Unit, Unit> DeselectAllAnimals { get; }

        public ReactiveCommand<Unit, Unit> DeselectAllSeries { get; }
        #endregion

        public ReactiveCommand<int, Unit> ZoomToAnimal { get; }

        public ReactiveCommand<int, Unit> DrawAnimalRouteCommand { get; }

        public Interaction<AddOrDeleteDialogWindowViewModel, string?> ShowRulerPointDialog { get; }

        public Interaction<AddRulerDialogWindowViewModel, MapRulerModel?> ShowAddRulerDialog { get; }

        private string? _selectedPoint;
        public string? SelectedPoint
        {
            get => _selectedPoint;
            set { this.RaiseAndSetIfChanged(ref _selectedPoint, value); }
        }

        #region Filtering
        private string _searchAnimal;
        public string SearchAnimal
        {
            get => _searchAnimal;
            set { this.RaiseAndSetIfChanged(ref _searchAnimal, value); }
        }
        Func<MapAnimalModel, bool> animalFilter(string text)
        {
            if (string.IsNullOrEmpty(text)) return animal => true;
            return animal => animal.ID.ToLower().Contains(text.ToLower());
        }

        private string _searchSeries;
        public string SearchSeries
        {
            get => _searchSeries;
            set { this.RaiseAndSetIfChanged(ref _searchSeries, value); }
        }
        Func<MapSeriesModel, bool> seriesFilter(string text)
        {
            if (string.IsNullOrEmpty(text)) return series => true;
            return series => series.Name.ToLower().Contains(text.ToLower());
        }

        private bool _showAnimalsFromSelectedSeriesOnly;
        public bool ShowAnimalsFromSelectedSeriesOnly
        {
            get => _showAnimalsFromSelectedSeriesOnly;
            set { this.RaiseAndSetIfChanged(ref _showAnimalsFromSelectedSeriesOnly, value); }
        }
        Func<MapAnimalModel, bool> showAnimalsFromSelectedSeriesOnlyFilter(bool enabled)
        {
            if (!enabled) return animal => true;
            var selectedSeries = _series.Items.Where(x => x.IsChecked);
            if (selectedSeries is null || selectedSeries.Count() == 0) return animal => false;
            return animal => selectedSeries.IntersectBy(animal.Series, x => x.Name).Count() > 0;
        }
        #endregion

        #region MapModes
        private bool _rulerMode;
        public bool RulerMode
        {
            get => _rulerMode;
            set { this.RaiseAndSetIfChanged(ref _rulerMode, value); }
        }

        public ICommand OpenLocalMap { get; }

        public ICommand OpenOSM { get; }
        #endregion


        #region DB Services
        private readonly IAnimalService _animalService;
        private readonly ICatchService _catchService;
        private readonly IPointService _pointService;
        private readonly ISeriesService _seriesService;
        #endregion


        public MapViewModel(IScreen screen, IAnimalService animalService, IPointService pointService, ICatchService catchService, ISeriesService seriesService)
        {
            HostScreen = screen;

            _animalService = animalService;
            _pointService = pointService;
            _catchService = catchService;
            _seriesService = seriesService;

            _animals = new SourceCache<MapAnimalModel, int>(x => x.TableID);

            _selectedPointAnimals = new SourceList<CatchPositionModel>();

            _series = new SourceCache<MapSeriesModel, int>(x => x.TableID);

            Observable.Start(() => {
                UpdateView();
            }, RxApp.TaskpoolScheduler);

            var map = CreateMap.CreateMap3857();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Widgets.Add(new ScaleBarWidget(map));
            map.Layers.Add(new Layer("rulerBookmark"));
            CurrentMapControl = new MapControl();
            CurrentMapControl.Map = map;

            CurrentMapControl.Map.Info += MapEventHandler;

            #region Changing Between Local And Online Maps
            OpenLocalMap = ReactiveCommand.CreateFromTask(async () =>
            {
                var storageProvider = Locator.Current.GetService<IStorageProvider>();

                var res = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Open Local Map File",
                    AllowMultiple = false,
                    FileTypeFilter = new FilePickerFileType[] { new("MapFile") { Patterns = new[] { "*.mbtiles" } }, FilePickerFileTypes.All }
                });

                if (res != null && res.Count != 0)
                {
                    var result = res.First();
                    var filePath = result.Path.LocalPath;

                    var conn = new SQLite.SQLiteConnectionString(filePath);

                    var tileSource = new MbTilesTileSource(conn);

                    CurrentMapControl.Map.Layers.Remove(CurrentMapControl.Map.Layers.First());
                    CurrentMapControl.Map.Layers.Insert(0, new TileLayer(tileSource) { Name = "MbTiles" });
                }
            });

            OpenOSM = ReactiveCommand.CreateFromTask(async () =>
            {
                CurrentMapControl.Map.Layers.Remove(CurrentMapControl.Map.Layers.First());
                CurrentMapControl.Map.Layers.Insert(0, OpenStreetMap.CreateTileLayer());
            });
            #endregion

            #region Animal and Series Lists

            _searchAnimal = string.Empty;
            var filterAnimalPredicate = this.WhenAnyValue(x => x.SearchAnimal)
              .DistinctUntilChanged()
              .Select(animalFilter);

            _searchSeries = string.Empty;
            var filterSeriesPredicate = this.WhenAnyValue(x => x.SearchSeries)
              .DistinctUntilChanged()
              .Select(seriesFilter);

            var filterShowSeriesPredicate = this.WhenAnyValue(x => x.ShowAnimalsFromSelectedSeriesOnly)
              .DistinctUntilChanged()
              .Select(showAnimalsFromSelectedSeriesOnlyFilter);

            _series.Connect()
                .Filter(filterSeriesPredicate)
                .Sort(SortExpressionComparer<MapSeriesModel>.Descending(t => t.IsChecked).ThenByAscending(t => t.Name.ToLower()))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _seriesList)
                .DisposeMany()
                .Subscribe();

            _selectedPointAnimals.Connect()
                .Sort(SortExpressionComparer<CatchPositionModel>.Ascending(t => t.Date))
                .Bind(out _selectedPointAnimalsList).Subscribe();

            _animals.Connect()
                .Filter(filterAnimalPredicate)
                .Filter(filterShowSeriesPredicate)
                .Sort(SortExpressionComparer<MapAnimalModel>.Descending(t => t.IsChecked).ThenByAscending(t => t.ID.ToLower()))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _animalsList)
                .DisposeMany()
                .Subscribe();
            #endregion

            #region Selection Commands
            SelectAllAnimals = ReactiveCommand.CreateFromTask(async () =>
            {
                _animals.Edit(innerCache =>
                {
                    foreach (var animal in innerCache.Items)
                        animal.IsChecked = true;
                });

                var stringSeries = GetCheckedSeriesNames();

                if (stringSeries == null || stringSeries.Count == 0)
                    return;

                UpdateAllCheckedAnimalsLayers();
            });

            SelectAllSeries = ReactiveCommand.CreateFromTask(async () =>
            {
                _series.Edit(innerCache =>
                {
                    foreach (var serie in innerCache.Items)
                        serie.IsChecked = true;
                });
                UpdateAllCheckedAnimalsLayers();
                UpdateAnimalListAfterSeriesChanged();
            });

            DeselectAllAnimals = ReactiveCommand.CreateFromTask(async () =>
            {
                var selectedAnimals = _animals.Items.Where(x => x.IsChecked);

                foreach (var animal in selectedAnimals)
                {
                    ClearSpecificAnimalLayer(animal);
                }

                _animals.Edit(innerCache =>
                {
                    foreach (var animal in selectedAnimals)
                        animal.IsChecked = false;
                });

                
            });

            DeselectAllSeries = ReactiveCommand.CreateFromTask(async () =>
            {
                _series.Edit(innerCache =>
                {
                    foreach (var serie in innerCache.Items)
                        serie.IsChecked = false;
                });

                UpdateAllCheckedAnimalsLayers();
                UpdateAnimalListAfterSeriesChanged();
            });
            #endregion

            #region Rulers

            ShowRulerPointDialog = new Interaction<AddOrDeleteDialogWindowViewModel, string?>();

            ShowAddRulerDialog = new Interaction<AddRulerDialogWindowViewModel, MapRulerModel?>();

            _rulers = new SourceCache<MapRulerModel, string>(x => x.Name);

            _rulers.Connect()
                .Bind(out _rulersList).Subscribe();

            _rulers.Connect().WhenPropertyChanged(item => item.IsChecked).Subscribe(change =>
            {
                if (change.Value)
                {
                    ShowLayer(change.Sender);
                }
                else
                {
                    HideLayer(change.Sender);
                }
            });

            AddRuler = ReactiveCommand.CreateFromTask(async () =>
            {
                MapRulerModel? result = await ShowAddRulerDialog.Handle(new AddRulerDialogWindowViewModel());
                if (result != null)
                {
                    if (string.IsNullOrEmpty(result.Name))
                        return;
                    else
                    {
                        if (_rulers.Keys.Contains(result.Name))
                        {
                            var box = MessageBoxManager
                                .GetMessageBoxStandard("", "Eror: Can't use the same name twice",
                                   ButtonEnum.Ok);
                            await box.ShowWindowAsync();
                        }
                        else
                        {
                            _rulers.AddOrUpdate(result);
                        }
                    }
                }
            });

            DeleteRuler = ReactiveCommand.CreateFromTask(async (string rulerName) =>
            {
                var box = MessageBoxManager
                        .GetMessageBoxStandard("", "Are you sure you want to delete this ruler?",
                        ButtonEnum.YesNo);

                var result = await box.ShowWindowAsync();

                if (result.Equals(ButtonResult.Yes))
                {
                    var mod = _rulers.Lookup(rulerName).Value;

                    var rulerLayer = mod.Layer;

                    if (rulerLayer != null && CurrentMapControl.Map.Layers.Contains(rulerLayer))
                        CurrentMapControl.Map.Layers.Remove(rulerLayer);

                    _rulers.Remove(rulerName);
                }
            });

            #endregion

            AnimalCheckedCommand = ReactiveCommand.CreateFromTask(async (int animalTableID) =>
            {
                var animal = _animals.Lookup(animalTableID).Value;

                if (animal.IsChecked)
                {
                    var stringSeries = GetCheckedSeriesNames();

                    if (stringSeries == null || stringSeries.Count == 0)
                        return;

                    UpdateCheckedAnimalLayer(animal, stringSeries);
                }
                else 
                {
                    ClearSpecificAnimalLayer(animal);
                }
                
            });

            SeriesCheckedCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                UpdateAnimalListAfterSeriesChanged();
                UpdateAllCheckedAnimalsLayers();
            });

            #region Animal Button Commands
            ZoomToAnimal = ReactiveCommand.CreateFromTask(async (int animalTableID) =>
            {
                var animal = _animals.Lookup(animalTableID).Value;
                if (animal.ActivePoints == null || animal.ActivePoints.Count == 0)
                {
                    var box = MessageBoxManager
                        .GetMessageBoxStandard("Error", "No visible points for this animal",
                        ButtonEnum.Ok);

                    await box.ShowWindowAsync();
                }
                else
                {
                    var sourcePoint = animal.ActivePoints.First();
                    MPoint mapPoint = SphericalMercator.FromLonLat(sourcePoint.Longitude, sourcePoint.Latitude).ToMPoint();
                    CurrentMapControl.Map.Navigator.CenterOnAndZoomTo(mapPoint, 2);
                }
            });

            DrawAnimalRouteCommand = ReactiveCommand.CreateFromTask(async (int animalTableID) =>
            {
                var animal = _animals.Lookup(animalTableID).Value;
                var animalRoute = animal.ActivePoints;
                if (animalRoute == null || animalRoute.Count == 0)
                {
                    var box = MessageBoxManager
                            .GetMessageBoxStandard("Error", "No visible points for this animal",
                            ButtonEnum.Ok);

                    await box.ShowWindowAsync();

                    return;
                }

                MapRulerModel? result = await ShowAddRulerDialog.Handle(new AddRulerDialogWindowViewModel(animal.ID));
                if (result != null)
                {
                    if (string.IsNullOrEmpty(result.Name))
                        return;
                    else
                    {
                        animalRoute.Sort();

                        result.Points = new List<PointModel>();
                        foreach (var point in animalRoute)
                        {
                            var pp = new PointModel();
                            pp.ID = point.Name;
                            pp.Latitude = point.Latitude;
                            pp.Longitude = point.Longitude;
                            result.Points.Add(pp);
                        }

                        if (_rulers.Keys.Contains(result.Name))
                        {
                            var box = MessageBoxManager
                                .GetMessageBoxStandard("", "Eror: Can't use the same name twice",
                                   ButtonEnum.Ok);
                            await box.ShowWindowAsync();
                        }
                        else
                        {
                            _rulers.AddOrUpdate(result);

                            var mod = _rulers.Lookup(result.Name).Value;

                            mod.Layer = CreateGeometryLayer.CreateRulerLayer(mod.Points, mod.Color);

                            CurrentMapControl.Map.Layers.Add(mod.Layer);
                        }
                    }
                }
            });
            #endregion

            
        }

        private Task UpdateAllCheckedAnimalsLayers()
        {
            return Task.Factory.StartNew(() =>
            {
                var animalsChecked = _animals.Items.Where(x => x.IsChecked);

                var stringSeries = GetCheckedSeriesNames();

                foreach (var animal in animalsChecked)
                {
                    UpdateCheckedAnimalLayer(animal, stringSeries);
                }
            });
        }

        private void UpdateCheckedAnimalLayer(MapAnimalModel animal, List<string>? stringSeries)
        {
            ClearSpecificAnimalLayer(animal);

            if (stringSeries is null || stringSeries.Count == 0)
            {
                return;
            }

            var animalCatches = _catchService.GetCatchesByAnimalAndSeriesWithPointAndSeries(animal.ID, stringSeries);

            if (animalCatches == null || animalCatches.Count == 0)
            {
                return;
            }

            foreach (var cat in animalCatches)
            {
                var datedPoint = new MapDatedPointModel(cat.Point, cat.Series, cat.Date);

                animal.ActivePoints ??= new();
                animal.ActivePoints.Add(datedPoint);
            }

            if (animal.ActivePoints is null || animal.ActivePoints.Count == 0)
            {
                animal.Layer = null;
                animal.ActivePoints = null;
            }
            else
            {
                animal.Layer = CreateGeometryLayer.CreateAnimalPositionsLayer(animal.ActivePoints);
                var bookmark = CurrentMapControl.Map.Layers.FindLayer("rulerBookmark").First();
                var index = CurrentMapControl.Map.Layers.IndexOf(bookmark);
                CurrentMapControl.Map.Layers.Insert(index, animal.Layer);
            }
        }

        private void ClearSpecificAnimalLayer(MapAnimalModel animal)
        {
            if (animal.Layer != null && CurrentMapControl.Map.Layers.Contains(animal.Layer))
                CurrentMapControl.Map.Layers.Remove(animal.Layer);
            animal.Layer = null;
            animal.ActivePoints = null;
        }

        private void UpdateAnimalListAfterSeriesChanged()
        {
            if (ShowAnimalsFromSelectedSeriesOnly)
            {
                ShowAnimalsFromSelectedSeriesOnly = false;
                ShowAnimalsFromSelectedSeriesOnly = true;
            }
        }

        private List<string>? GetCheckedSeriesNames()
        {
            var seriesChecked = _series.Items.Where(x => x.IsChecked);
            if (seriesChecked == null || seriesChecked.Count() == 0)
                return null;
            var stringSeries = new List<string>();
            foreach (var ser in seriesChecked)
            {
                stringSeries.Add(ser.Name);
            }
            return stringSeries;
        }

        async void MapEventHandler(object? sender, MapInfoEventArgs e)
        {
            if (e == null)
            {
                e.Handled = true;
                return;
            }
            if (e.MapInfo.Feature == null)
            {
                e.Handled = true;
                return;
            }
            if (e.NumTaps >= 1)
            {
                var feature = e.MapInfo.Feature;
                if (feature is GeometryFeature)
                {
                    if ((feature as GeometryFeature).Geometry.UserData == null)
                    {
                        e.Handled = true;
                        return;
                    }
                    var point = (feature as GeometryFeature).Geometry.UserData.ToString();
                    var pointModel = _pointService.GetByIDWithChildrenAndOtherParents(point);

                    if (RulerMode)
                    {
                        await HandleRuler(pointModel);
                    }
                    if (e.NumTaps > 1)
                    {
                        SelectedPoint = new string(point);

                        _selectedPointAnimals.Clear();

                        var pointCatches = pointModel.Catches;

                        if (pointCatches == null || pointCatches.Count == 0)
                        {
                            e.Handled = true;
                            return;
                        }

                        var catchPositions = new List<CatchPositionModel>();

                        foreach (var cat in pointCatches)
                        {
                            catchPositions.Add(new CatchPositionModel(cat.Animal, pointModel, cat));
                        }

                        _selectedPointAnimals.AddRange(catchPositions);
                    }
                }
            }
            e.Handled = true;
            return;
        }

        private async Task HandleRuler(PointModel pointModel)
        {
            if (SelectedRuler == null)
            {
                var box = MessageBoxManager
                        .GetMessageBoxStandard("Error", "No ruler selected",
                        ButtonEnum.Ok);

                await box.ShowWindowAsync();

                return;
            }

            var mod = _rulers.Lookup(SelectedRuler.Name).Value;

            if (mod.IsChecked == false)
            {
                var box = MessageBoxManager
                        .GetMessageBoxStandard("Error", "Ruler is currently hidden, make it visible to edit",
                        ButtonEnum.Ok);

                await box.ShowWindowAsync();

                return;
            }

            if (mod.Points.Contains(pointModel))
            {
                string? result = await ShowRulerPointDialog.Handle(new AddOrDeleteDialogWindowViewModel());
                if (result != null)
                {
                    if (result.Equals("Add"))
                        mod.Points.Add(pointModel);
                    else if (result.Equals("Delete"))
                        mod.Points.Remove(pointModel);
                }
            }
            else
            {
                mod.Points.Add(pointModel);
            }

            if (mod.Layer != null && CurrentMapControl.Map.Layers.Contains(mod.Layer))
                CurrentMapControl.Map.Layers.Remove(mod.Layer);
            mod.Layer = CreateGeometryLayer.CreateRulerLayer(mod.Points, SelectedRuler.Color);
            CurrentMapControl.Map.Layers.Add(mod.Layer);
        }

        private void ShowLayer(IModelWithLayer modelWithLayer)
        {
            if (modelWithLayer.Layer != null)
                CurrentMapControl.Map.Layers.Add(modelWithLayer.Layer);
        }

        private void HideLayer(IModelWithLayer modelWithLayer)
        {
            if (modelWithLayer.Layer != null)
                CurrentMapControl.Map.Layers.Remove(modelWithLayer.Layer);
        }

        public void UpdateView()
        {
            var seriesSource = _seriesService.GetAllWithChildren();
            _series.Edit(innerCache =>
            {
                innerCache.Clear();
                foreach (var series in seriesSource)
                {
                    innerCache.AddOrUpdate(new MapSeriesModel(series));
                }
            });

            if (CurrentMapControl is not null)
            {
                foreach (var layer in CurrentMapControl.Map.Layers.Skip(1))
                {
                    if (layer.Name.Equals("rulerBookmark"))
                        break;
                    else
                        CurrentMapControl.Map.Layers.Remove(layer);
                }
            }

            var animalSource = _animalService.GetAllWithCatchesAndCatchSeries();

            _animals.Edit(innerList =>
            {
                innerList.Clear();
                foreach (var animal in animalSource)
                {
                    var mapModel = new MapAnimalModel(animal);
                    if (animal.Catches.Count == 0)
                        continue;
                    foreach (var cat in animal.Catches)
                    {
                        var seriesID = cat.Series.ID;
                        if (!mapModel.Series.Contains(seriesID))
                            mapModel.Series.Add(seriesID);
                    }
                    innerList.AddOrUpdate(mapModel);
                }
            });

            ShowAnimalsFromSelectedSeriesOnly = true;
        }
    }
}
