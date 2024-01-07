using ReactiveUI;
using System.Reactive;

namespace Raton.Map.ViewModels.Dialogs
{
    public class AddOrDeleteDialogWindowViewModel : ReactiveObject
    {
        public ReactiveCommand<Unit, string> Add { get; }
        public ReactiveCommand<Unit, string> Delete { get; }
        public ReactiveCommand<Unit, string> Close { get; }

        public AddOrDeleteDialogWindowViewModel()
        {
            Add = ReactiveCommand.Create(() =>
            {
                return "Add";
            });

            Delete = ReactiveCommand.Create(() =>
            {
                return "Delete";
            });

            Close = ReactiveCommand.Create(() =>
            {
                return "Close";
            });
        }

    }
}
