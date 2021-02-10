using Prism.Mvvm;
using Sharp.FileSystem.Smb;
using Sharp.FileSystem.Smb.Discovery;
using Sharp.FileSystems.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace FileBrowser.ViewModel
{
    public class FileBrowserViewModel : BindableBase
    {
        private readonly SmbFileSystem _fileSystem;
        

        public FileBrowserViewModel()
        {
            _fileSystem = new SmbFileSystem();
            CurrentDirectory = _fileSystem.DirectoryInfo.FromDirectoryName("smb://corenode/shared");

            
        }

        private IDirectoryInfo _currentDirectory;      

        public IDirectoryInfo CurrentDirectory
        {
            get => _currentDirectory; 
            set => SetProperty(ref _currentDirectory, value);
        }

        private IDisposable _sc;
    }
}
