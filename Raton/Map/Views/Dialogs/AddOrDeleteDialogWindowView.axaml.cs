using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Raton.Map.ViewModels.Dialogs;
using ReactiveUI;
using System;

namespace Raton.Views.Dialogs;

public partial class AddOrDeleteDialogWindowView : ReactiveWindow<AddOrDeleteDialogWindowViewModel>
{
    public AddOrDeleteDialogWindowView()
    {
        AvaloniaXamlLoader.Load(this);
        this.WhenActivated(d => d(ViewModel!.Add.Subscribe(Close)));
        this.WhenActivated(d => d(ViewModel!.Delete.Subscribe(Close)));
        this.WhenActivated(d => d(ViewModel!.Close.Subscribe(Close)));
    }
}