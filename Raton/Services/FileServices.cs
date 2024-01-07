using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;

namespace Raton.Services
{
    public static class FileServices
    {
        public static async Task DoShowFileDialogAsync(InteractionContext<Unit,
                                        IStorageFile?> interaction, Window source)
        {
            var topLevel = TopLevel.GetTopLevel(source);

            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Animal Data File",
                AllowMultiple = false,
                FileTypeFilter = new FilePickerFileType[] { new("ExcelFile") { Patterns = new[] { "*.xlsx" } }, FilePickerFileTypes.All }
            });
            if (files.Count >= 1)
            {
                interaction.SetOutput(files[0]);
            }
            else
            {
                interaction.SetOutput(null);
            }
        }
    }
}
