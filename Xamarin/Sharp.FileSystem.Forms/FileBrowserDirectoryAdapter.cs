using Sharp.FileSystem.Forms.ViewModels;
using Sharp.FileSystems.Abstractions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Sharp.FileSystem.Forms
{
    public class FileBrowserDirectoryAdapterErrorEventArgs : EventArgs
    {
        public FileBrowserDirectoryAdapterErrorEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }

    public class FileBrowserDirectoryAdapter : INotifyPropertyChanged, IFileBrowserDirectoryAdapter
    {
        enum ViewState
        {
            FileSystemListing,
            LogicalDriveListing,
            DirectoryListing
        }

        private readonly IFileSystemDiscovery[] _fileSystemDiscoverers;
        private readonly List<IDisposable> _networkDiscoverySubscriptions = new List<IDisposable>();
        private ViewState _viewState = ViewState.FileSystemListing;
        private FileSystemRootItem _currentRootItem;


        public Func<FileBrowserDirectoryAdapterErrorEventArgs, Task> Error;
        public event PropertyChangedEventHandler PropertyChanged;

        public FileBrowserDirectoryAdapter(params IFileSystemDiscovery[] fileSystemDiscoverers)
        {
            _fileSystemDiscoverers = fileSystemDiscoverers;
            BindingBase.EnableCollectionSynchronization(_items, null, ObservableCollectionCallback);
        }



        public virtual async Task SetDirectoryAsync(IDirectoryInfo directoryInfo)
        {
            if (directoryInfo == null)
            {
                switch (_viewState)
                {
                    case ViewState.LogicalDriveListing:
                        _items.Clear();
                        StartDiscovery();
                        break;
                    case ViewState.DirectoryListing:
                        SetDiscoveredItems(_currentRootItem);
                        break;
                }

            }
            else
            {
                _viewState = ViewState.DirectoryListing;
                var items = new List<FileSystemItem>();

                await Task.Run(() =>
                {
                    foreach (var item in directoryInfo.EnumerateFileSystemInfos())
                    {
                        var vitem = CreateFileSystemItem(item);
                        if (vitem != null)
                        {
                            items.Add(vitem);
                        }
                    }
                    items = SortItems(items).ToList();
                    var up = CreateDirectoryUpFileSystemItem(directoryInfo.Parent);
                    if (up != null)
                    {
                        items.Insert(0, up);
                    }
                });
                _items.Clear();
                foreach (var item in items)
                {
                    _items.Add(item);
                }
                OnPropertyChanged(nameof(Items));
            }
        }

        protected virtual IEnumerable<FileSystemItem> SortItems(IEnumerable<FileSystemItem> items)
        {
            return items.OrderBy(i => i is DirectoryItem ? 0 : 1).ThenBy(i => i.Name);
        }

        protected virtual FileSystemItem CreateDirectoryUpFileSystemItem(IDirectoryInfo directoryInfo)
        {
            return new DirectoryUpItem(directoryInfo);
        }

        protected virtual FileSystemItem CreateFileSystemItem(IFileSystemInfo fileSystemInfo)
        {
            if (fileSystemInfo is IDirectoryInfo dirInfo)
            {
                return new DirectoryItem(dirInfo);
            }
            else if (fileSystemInfo is IFileInfo fileInfo)
            {
                return new FileItem(fileInfo);
            }
            return null;
        }

        protected virtual FileSystemItem CreateFileSystemItem(IFileSystemDiscoveryResult discoveryResult)
        {
            return new FileSystemRootItem(discoveryResult);
        }

        public virtual void StartDiscovery()
        {
            _viewState = ViewState.FileSystemListing;
            foreach (var discovery in _fileSystemDiscoverers)
            {
                _networkDiscoverySubscriptions.Add(discovery
                    .DiscoverRootDirectoriesContinuous()
                    .Subscribe(OnFileSystemDiscovered));
            }
        }

        protected virtual void OnFileSystemDiscovered(IFileSystemDiscoveryResult result)
        {
            lock (_items)
            {
                var existing = _items.OfType<FileSystemRootItem>().FirstOrDefault(f => f.FileSystemDiscoveryResult.Equals(result));
                if (existing == null)
                {
                    // TODO sorting
                    _items.Add(CreateFileSystemItem(result));
                }
            }

            // HACK: iOS wants that - why?
            OnPropertyChanged(nameof(Items));
        }

        protected virtual void SetDiscoveredItems(FileSystemRootItem rootItem)
        {
            try
            {
                StopDiscovery();
                _viewState = ViewState.LogicalDriveListing;
                var fsdr = rootItem.FileSystemDiscoveryResult;
                var drives = fsdr.FileSystem.Directory.GetLogicalDrives(fsdr.RootPath);

                lock (_items)
                {
                    _items.Clear();
                    _items.Add(new DirectoryUpItem(null));
                    foreach (var drive in drives)
                    {
                        var dirinfo = fsdr.FileSystem.DirectoryInfo.FromDirectoryName(fsdr.FileSystem.Path.Combine(fsdr.RootPath, drive), false);
                        _items.Add(CreateFileSystemItem(dirinfo));
                    }
                }
                OnPropertyChanged(nameof(Items));
            }
            catch
            {
                StartDiscovery();
            }
        }

        public virtual async Task OpenItemAsync(FileSystemItem fileSystemItem)
        {
            if (fileSystemItem is DirectoryItem directoryItem)
            {
                await SetDirectoryAsync(directoryItem.DirInfo);
            }
            else if (fileSystemItem is FileSystemRootItem rootItem)
            {
                _currentRootItem = rootItem;
                SetDiscoveredItems(rootItem);
            }
        }

        public virtual void StopDiscovery()
        {
            foreach (var subscription in _networkDiscoverySubscriptions)
            {
                subscription.Dispose();
            }
            _networkDiscoverySubscriptions.Clear();
        }




        private readonly ObservableCollection<FileSystemItem> _items = new ObservableCollection<FileSystemItem>();


        public IEnumerable<FileSystemItem> Items
        {
            get => _items;
        }

        private void ObservableCollectionCallback(IEnumerable collection, object context, Action accessMethod, bool writeAccess)
        {
            lock (collection)
            {
                accessMethod();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopDiscovery();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public async Task OnErrorAsync(Exception exception)
        {
            await Error?.Invoke(new FileBrowserDirectoryAdapterErrorEventArgs(exception));
        }
    }
}
