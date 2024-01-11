using Avalonia.Platform.Storage;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using Raton.Services.DbServices;
using Raton.Views;
using Raton.Tables.ViewModels;
using ReactiveUI;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using System;
using Splat;
using Raton.ExportAndImport;

namespace Raton.ViewModels
{
    public class MainWindowViewModel : ReactiveObject, IScreen
    {
        #region Routing
        public RoutingState Router { get; } = new RoutingState();

        public ReactiveCommand<Unit, IRoutableViewModel> GoToMap { get; }

        public ReactiveCommand<Unit, IRoutableViewModel> GoToAnimalTable { get; }

        public ReactiveCommand<Unit, IRoutableViewModel> GoToPointTable { get; }

        public ReactiveCommand<Unit, IRoutableViewModel> GoToCatchTable { get; }

        public ReactiveCommand<Unit, IRoutableViewModel> GoToSeriesTable { get; }
        #endregion

        #region DB Services
        private DataContext db;
        private IAnimalService _animalService;
        private ICatchService _catchService;
        private IPointService _pointService;
        private ISeriesService _seriesService;
        #endregion

        #region File Commands and Interactions
        public ICommand OpenFileCommand { get; }
        public ICommand SaveFileCommand { get; }
        public ICommand DropDbCommand { get; }
        #endregion

        public MainWindowViewModel(DataContext dataBase)
        {
            db = dataBase;

            _animalService = new AnimalService(dataBase);
            _pointService = new PointService(dataBase);
            _catchService = new CatchService(dataBase);
            _seriesService = new SeriesService(dataBase);

            #region File Commands and Interactions
            OpenFileCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var storageProvider = Locator.Current.GetService<IStorageProvider>();

                if (storageProvider is null)
                    throw new NotImplementedException();

                var res = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Open Animal Data File",
                    AllowMultiple = false,
                    FileTypeFilter = new FilePickerFileType[] { new("ExcelFile") { Patterns = new[] { "*.xlsx" } }, FilePickerFileTypes.All }
                });

                if (res is not null && res.Count != 0)
                {
                    var result = res.First();
                    var filePath = result.Path.LocalPath;
                    var importResult = await Excel.ImportXlsx(filePath, _animalService, _pointService, _catchService, _seriesService);

                    if (Router.NavigationStack.Count > 0)
                    {
                        var vm = Router.NavigationStack.Last();
                        if (vm is not null && vm is IUpdatable)
                            (vm as IUpdatable).UpdateView();
                    }

                    var box = MessageBoxManager
                        .GetMessageBoxStandard("Import finished",
                        "Result: " + importResult,
                        ButtonEnum.Ok);

                    await box.ShowWindowAsync();
                }
            });

            SaveFileCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var storageProvider = Locator.Current.GetService<IStorageProvider>();

                if (storageProvider is null)
                    throw new NotImplementedException();

                var res = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Select a place to save your file",
                    SuggestedFileName = "ratonExport",
                    DefaultExtension = "xlsx",
                    FileTypeChoices = new FilePickerFileType[] { new("ExcelFile") { Patterns = new[] { "*.xlsx" } }, FilePickerFileTypes.All },
                    ShowOverwritePrompt = true
                });

                if (res is not null)
                {
                    var filePath = res.Path.LocalPath;
                    
                    Excel.ExportToXlsx(filePath, _animalService, _pointService, _catchService, _seriesService);

                    var box = MessageBoxManager
                        .GetMessageBoxStandard("Export finished",
                        "Export finished",
                        ButtonEnum.Ok);

                    await box.ShowWindowAsync();
                }
            });

            DropDbCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var box = MessageBoxManager
                        .GetMessageBoxStandard("Clear Database", 
                        "Are you sure you want to delete all records in database?",
                        ButtonEnum.YesNo);

                var deleteConfirmation = await box.ShowWindowAsync();

                if (deleteConfirmation.Equals(ButtonResult.Yes))
                {
                    db.Clear();

                    var boxFin = MessageBoxManager
                        .GetMessageBoxStandard("Success",
                        "Database Cleared",
                        ButtonEnum.Ok);

                    await boxFin.ShowWindowAsync();
                }

                if (Router.NavigationStack.Count > 0)
                {
                    var vm = Router.NavigationStack.Last();
                    if (vm is IUpdatable)
                        (vm as IUpdatable).UpdateView();
                }
            });
            #endregion

            #region Routing Commands
            GoToMap = ReactiveCommand.CreateFromObservable(
                () => Router.NavigateAndReset.Execute(new Map.ViewModels.MapViewModel(this, _animalService, _pointService, _catchService, _seriesService))
            );

            GoToAnimalTable = ReactiveCommand.CreateFromObservable(
                () => Router.NavigateAndReset.Execute(new AnimalTableViewModel(this, _animalService))
            );

            GoToPointTable = ReactiveCommand.CreateFromObservable(
                () => Router.NavigateAndReset.Execute(new PointTableViewModel(this, _pointService))
            );

            GoToCatchTable = ReactiveCommand.CreateFromObservable(
                () => Router.NavigateAndReset.Execute(new CatchTableViewModel(this, _animalService, _pointService, _catchService, _seriesService))
            );

            GoToSeriesTable = ReactiveCommand.CreateFromObservable(
                () => Router.NavigateAndReset.Execute(new SeriesTableViewModel(this, _seriesService))
            );

            #endregion

        }
    }
}