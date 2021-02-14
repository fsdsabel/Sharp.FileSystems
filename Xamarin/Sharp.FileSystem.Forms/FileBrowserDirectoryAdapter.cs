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
