using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Raton.Tables.ViewModels;
using ReactiveUI;

namespace Raton.Tables.Views
{
    public partial class PointTableView : ReactiveUserControl<PointTableViewModel>
    {
        public PointTableView()
        {
            AvaloniaXamlLoader.Load(this);

            this.WhenActivated(disposables => { });
        }
    }
}
