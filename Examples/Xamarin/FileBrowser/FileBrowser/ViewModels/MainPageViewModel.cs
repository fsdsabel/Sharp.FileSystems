using Prism.Mvvm;
using Prism.Services;
using Sharp.FileSystem.Forms;
using Sharp.FileSystem.Forms.ViewModels;
using Sharp.FileSystem.Smb;
using Sharp.FileSystem.Smb.Discovery;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace FileBrowser.ViewModels
{
    public class MainPageViewModel : BindableBase
    {
        private readonly SmbFileSystem _fileSystem;
        private readonly IPageDialogService _pageDialogService;

        public MainPageViewModel(IPageDialogService pageDialogService)
        {
            _pageDialogService = pageDialogService;
            _fileSystem = new SmbFileSystem();
            Adapter = new FileBrowserDirectoryAdapter(new SmbFileSystemDiscovery());
            Adapter.Error = OnFileBrowserError;
            OpenFileCommand = new Command<FileItem>(OnOpenFile);
        }

        private async void OnOpenFile(FileItem file)
        {
            await _pageDialogService.DisplayAlertAsync("Open file", $"You clicked on the file '{file.FileInfo.Uri}'", "OK");
        }

        private async Task OnFileBrowserError(FileBrowserDirectoryAdapterErrorEventArgs e)
        {
            await _pageDialogService.DisplayAlertAsync("Error", e.Exception.Message, "Continue");
        }

        public ICommand OpenFileCommand { get; }

        public FileBrowserDirectoryAdapter Adapter { get; }
    }
}
