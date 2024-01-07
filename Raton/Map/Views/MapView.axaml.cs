using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Raton.Map.Models;
using Raton.Map.ViewModels;
using Raton.Map.ViewModels.Dialogs;
using Raton.Map.Views.Dialogs;
using Raton.Views.Dialogs;
using ReactiveUI;
using System.Threading.Tasks;

namespace Raton.Map.Views;

public partial class MapView : ReactiveUserControl<MapViewModel>
{
    public MapView()
    {
        AvaloniaXamlLoader.Load(this);

        this.WhenActivated(action =>
            action(ViewModel!.ShowRulerPointDialog.RegisterHandler(DoShowRulerPointDialogAsync)));

        this.WhenActivated(action =>
            action(ViewModel!.ShowAddRulerDialog.RegisterHandler(DoShowAddRulerDialogAsync)));
    }

    private async Task DoShowRulerPointDialogAsync(InteractionContext<AddOrDeleteDialogWindowViewModel,
                                        string?> interaction)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        var dialog = new AddOrDeleteDialogWindowView();
        dialog.DataContext = interaction.Input;

        var result = await dialog.ShowDialog<string?>(topLevel as Window);
        interaction.SetOutput(result);
    }

    private async Task DoShowAddRulerDialogAsync(InteractionContext<AddRulerDialogWindowViewModel,
                                        MapRulerModel?> interaction)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        var dialog = new AddRulerDialogWindowView();
        dialog.DataContext = interaction.Input;

        var result = await dialog.ShowDialog<MapRulerModel?>(topLevel as Window);
        interaction.SetOutput(result);
    }

    private void Binding(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
    }
}