using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Raton.Tables.ViewModels;
using ReactiveUI;

namespace Raton.Tables.Views;

public partial class CatchTableView : ReactiveUserControl<CatchTableViewModel>
{
    public CatchTableView()
    {
        AvaloniaXamlLoader.Load(this);

        this.WhenActivated(disposables => { });
    }
}