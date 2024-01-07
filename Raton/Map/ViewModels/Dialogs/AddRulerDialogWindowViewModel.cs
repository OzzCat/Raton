using Avalonia.Media;
using Raton.Map.Models;
using ReactiveUI;
using System.Reactive;

namespace Raton.Map.ViewModels.Dialogs
{
    public class AddRulerDialogWindowViewModel : ReactiveObject
    {
        public ReactiveCommand<Unit, MapRulerModel> Add { get; }
        public ReactiveCommand<Unit, MapRulerModel> Close { get; }

        private string _name;
        public string Name
        {
            get => _name;
            set 
            { 
                this.RaiseAndSetIfChanged(ref _name, value);
                EnableSaving = !string.IsNullOrEmpty(value);
            }
        }

        private bool _enableSaving = false;
        public bool EnableSaving
        {
            get => _enableSaving;
            set => this.RaiseAndSetIfChanged(ref _enableSaving, value);
        }
        public Color SelectedColor { get; set; }

        public AddRulerDialogWindowViewModel()
        {
            Add = ReactiveCommand.Create(() =>
            {
                return new MapRulerModel(Name, SelectedColor.A, SelectedColor.R, SelectedColor.G, SelectedColor.B);
            });

            Close = ReactiveCommand.Create(() =>
            {
                return new MapRulerModel(string.Empty, 0, 0, 0, 0);
            });
        }

        public AddRulerDialogWindowViewModel(string name)
        {
            _name = name;
            _enableSaving = true;

            Add = ReactiveCommand.Create(() =>
            {
                return new MapRulerModel(Name, SelectedColor.A, SelectedColor.R, SelectedColor.G, SelectedColor.B);
            });

            Close = ReactiveCommand.Create(() =>
            {
                return new MapRulerModel(string.Empty, 0, 0, 0, 0);
            });
        }
    }
}
