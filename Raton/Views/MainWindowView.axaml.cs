using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Raton.ViewModels;

namespace Raton.Views
{
    public partial class MainWindowView : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindowView()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}