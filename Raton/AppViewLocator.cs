using Raton.Map.ViewModels;
using Raton.Map.Views;
using Raton.Tables.ViewModels;
using Raton.Tables.Views;
using ReactiveUI;
using System;

namespace Raton
{
    public class AppViewLocator : ReactiveUI.IViewLocator
    {
        public IViewFor ResolveView<T>(T viewModel, string contract = null) => viewModel switch
        {
            AnimalTableViewModel context => new AnimalTableView { DataContext = context },
            MapViewModel context => new MapView { DataContext = context },
            PointTableViewModel context => new PointTableView { DataContext = context },
            CatchTableViewModel context => new CatchTableView { DataContext = context },
            SeriesTableViewModel context => new SeriesTableView { DataContext = context },
            _ => throw new ArgumentOutOfRangeException(nameof(viewModel))
        };
    }
}
