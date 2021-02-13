using Prism.Mvvm;
using Prism.Services;
using Sharp.FileSystem.Forms;
using Sharp.FileSystem.Smb;
using Sharp.FileSystem.Smb.Discovery;
using Sharp.FileSystems.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

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
            //CurrentDirectory = _fileSystem.DirectoryInfo.FromDirectoryName("smb://corenode/shared");
            Adapter = new FileBrowserDirectoryAdapter(new SmbFileSystemDiscovery());
            Adapter.Error = OnFileBrowserError;
            Adapter.StartDiscovery();
        }

        private async Task OnFileBrowserError(FileBrowserDirectoryAdapterErrorEventArgs e)
        {
            await _pageDialogService.DisplayAlertAsync("Error", e.Exception.Message, "Continue");
        }

        private IDirectoryInfo _currentDirectory;      

        public IDirectoryInfo CurrentDirectory
        {
            get => _currentDirectory; 
            set => SetProperty(ref _currentDirectory, value);
        }


        public FileBrowserDirectoryAdapter Adapter { get; }
    }
}
