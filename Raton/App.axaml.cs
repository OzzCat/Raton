using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Raton.Services.DbServices;
using Raton.ViewModels;
using Raton.Views;
using Splat;

namespace Raton
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindowView
                {
                    DataContext = new MainWindowViewModel(new DataContext()),
                };

                Locator.CurrentMutable.RegisterConstant(desktop);

                Locator.CurrentMutable.RegisterConstant(desktop.MainWindow.StorageProvider);
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}