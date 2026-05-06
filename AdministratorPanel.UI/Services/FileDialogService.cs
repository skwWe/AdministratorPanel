using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using System.Linq;
using System.Threading.Tasks;

namespace AdministratorPanel.UI.Services
{
    public sealed class FileDialogService : IFileDialogService
    {
        public async Task<string?> PickExcelFileAsync()
        {
            if (Avalonia.Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
                return null;

            var window = desktop.MainWindow;

            if (window is null)
                return null;

            var files = await window.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = "Выберите Excel-файл плана отключения",
                    AllowMultiple = false,
                    FileTypeFilter =
                    [
                        new FilePickerFileType("Excel файлы")
                        {
                            Patterns = ["*.xlsx", "*.xls"]
                        }
                    ]
                });

            var file = files.FirstOrDefault();

            return file?.Path.LocalPath;
        }
    }
}