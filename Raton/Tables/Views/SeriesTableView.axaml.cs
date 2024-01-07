using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Raton.Tables.ViewModels;
using ReactiveUI;

namespace Raton.Tables.Views;

public partial class SeriesTableView : ReactiveUserControl<SeriesTableViewModel>
{
    public SeriesTableView()
    {
        AvaloniaXamlLoader.Load(this);

        this.WhenActivated(disposables => {
        });
    }
}