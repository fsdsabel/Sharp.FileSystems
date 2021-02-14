using Sharp.FileSystem.Forms.ViewModels;
using Sharp.FileSystems.Abstractions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
        /*enum ViewState
        {
            FileSystemListing,
            LogicalDriveListing,
            DirectoryListing
        }*/


     //   private ViewState _viewState = ViewState.FileSystemListing;
        private FileSystemItem _currentRootItem;


        public Func<FileBrowserDirectoryAdapterErrorEventArgs, Task> Error;
        private FileSystemItemBase _currentDirectory;
        private IFileSystemDiscovery[] _fileSystemDiscoverers;

        public event PropertyChangedEventHandler PropertyChanged;

        public FileBrowserDirectoryAdapter(params IFileSystemDiscovery[] fileSystemDiscoverers)
        {
            _fileSystemDiscoverers = fileSystemDiscoverers;
            BindingBase.EnableCollectionSynchronization(_items, null, ObservableCollectionCallback);
            NavigateToFileSystemItemAsync(CreateRootItem(_fileSystemDiscoverers));
        }

        protected virtual async Task NavigateToFileSystemItemAsync(FileSystemItemBase item)
        {
            bool attachChildrenChangedHandler = true;
            if (_currentDirectory != null && _currentDirectory != item)
            {
                _currentDirectory.ChildrenChanged -= OnChildrenChanged;
                attachChildrenChangedHandler = false;
            }
            foreach (var oldItem in _items)
            {
                oldItem.Dispose();
            }
            var children = SortItems((await item.EnumerateChildrenAsync())).ToArray();
            lock (_items)
            {
                _items.Clear();
                foreach (var child in children)
                {
                    _items.Add(child);
                }
            }
            if (attachChildrenChangedHandler)
            {
                item.ChildrenChanged += OnChildrenChanged;
            }

            _currentDirectory = item;
            // iOS requires this
            OnPropertyChanged(nameof(Items));
        }

        private void OnChildrenChanged(object sender, EventArgs e)
        {
            RefreshAsync();
        }

        public async Task OpenItemAsync(FileSystemItemBase fileSystemItem)
        {
            await NavigateToFileSystemItemAsync(fileSystemItem);
        }

        public Task RefreshAsync()
        {
            return NavigateToFileSystemItemAsync(_currentDirectory);
        }

     
        protected virtual IEnumerable<FileSystemItemBase> SortItems(IEnumerable<FileSystemItemBase> items)
        {
            return items.OrderBy(i =>
            {
                if (i is DirectoryUpItem) return 0;
                if (i is DirectoryItem) return 1;
                return 2;
            }).ThenBy(i => i.Name);
        }

        protected virtual FileSystemItemBase CreateRootItem(IFileSystemDiscovery[] fileSystemDiscoverers)
        {
            return new FileSystemRootItem(fileSystemDiscoverers);
        }
    

        /*

        protected virtual async Task SetDirectoryAsync(IDirectoryInfo directoryInfo)
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
                    var up = CreateDirectoryUpFileSystemItem(CurrentDirectory);
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

        protected virtual FileSystemItem CreateDirectoryUpFileSystemItem(FileSystemItem parentItem)
        {
            if(parentItem is DirectoryItem directoryItem)
            {
                return new DirectoryUpItem(directoryItem.DirInfo);
            }
            if (parentItem is FileSystemRootItem rootItem)
            {
                //return new DirectoryUpItem(rootItem.FileSystemDiscoveryResult.);
            }
            return new DirectoryUpItem(null);
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
            CurrentDirectory = null;
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
                    _items.Add(CreateDirectoryUpFileSystemItem(null));
                    foreach (var drive in drives)
                    {
                        var dirinfo = fsdr.FileSystem.DirectoryInfo.FromDirectoryName(fsdr.FileSystem.Path.Combine(fsdr.RootPath, drive), false);
                        _items.Add(CreateFileSystemItem(dirinfo));
                    }
                }
                OnPropertyChanged(nameof(Items));
                CurrentDirectory = rootItem;
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
                CurrentDirectory = new DirectoryItem(directoryItem.DirInfo); // may be DirectoryUpItem
            }
            else if (fileSystemItem is FileSystemRootItem rootItem)
            {
                _currentRootItem = rootItem;
                SetDiscoveredItems(rootItem);
                CurrentDirectory = fileSystemItem;
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

        public virtual async Task RefreshAsync()
        {
            await OpenItemAsync(CurrentDirectory);
        }*/


        private readonly ObservableCollection<FileSystemItemBase> _items = new ObservableCollection<FileSystemItemBase>();


        public IEnumerable<FileSystemItemBase> Items
        {
            get
            {
                lock (_items)
                {
                    return _items.ToArray();
                }
            }
        }

        public virtual FileSystemItemBase CurrentDirectory
        {
            get => _currentDirectory;
            set
            {
                if(_currentDirectory != value)
                {
                    _currentDirectory = value;
                    OnPropertyChanged();
                }
            }
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
           if(disposing)
            {
                foreach(var item in _items)
                {
                    item.Dispose();
                }
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
