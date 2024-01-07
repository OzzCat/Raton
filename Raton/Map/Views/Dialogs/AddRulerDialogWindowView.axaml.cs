using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Raton.Map.ViewModels.Dialogs;
using ReactiveUI;
using System;

namespace Raton.Map.Views.Dialogs;

public partial class AddRulerDialogWindowView : ReactiveWindow<AddRulerDialogWindowViewModel>
{
    public AddRulerDialogWindowView()
    {
        AvaloniaXamlLoader.Load(this);
        this.WhenActivated(d => d(ViewModel!.Add.Subscribe(Close)));
        this.WhenActivated(d => d(ViewModel!.Close.Subscribe(Close)));
    }
}