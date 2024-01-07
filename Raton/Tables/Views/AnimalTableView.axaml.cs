using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Raton.Tables.ViewModels;
using ReactiveUI;

namespace Raton.Tables.Views;

public partial class AnimalTableView : ReactiveUserControl<AnimalTableViewModel>
{
    public AnimalTableView()
    {
        AvaloniaXamlLoader.Load(this);

        this.WhenActivated(disposables => {
        });
    }
}