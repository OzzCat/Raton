using ReactiveUI;

namespace Raton.Tables.Templates.ViewModels
{
    public class CustomHeader : ReactiveObject
    {
        public string Title { get; set; }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { this.RaiseAndSetIfChanged(ref _searchText, value); }
        }

        public CustomHeader(string title)
        {
            Title = title;
            _searchText = string.Empty;
        }
    }
}
