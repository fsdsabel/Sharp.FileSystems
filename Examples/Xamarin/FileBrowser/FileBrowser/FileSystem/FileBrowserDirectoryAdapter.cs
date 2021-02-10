using Prism.Mvvm;
using Sharp.FileSystems.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileBrowser.FileSystem
{
    public class FileBrowserDirectoryAdapter : BindableBase
    {
        public virtual async Task SetDirectoryAsync(IDirectoryInfo directoryInfo)
        {
            if (directoryInfo == null)
            {
                Items = null;
            }
            else
            {
                var items = new List<FileSystemItem>();

                await Task.Run(() =>
                {
                    foreach (var item in directoryInfo.EnumerateFileSystemInfos())
                    {
                        var vitem = CreateFileSystemItem(item);
                        if(vitem != null)
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
                Items = items;
            }
        }

        protected virtual IEnumerable<FileSystemItem> SortItems(IEnumerable<FileSystemItem> items)
        {
            return items.OrderBy(i => i is DirectoryItem ? 0 : 1).ThenBy(i => i.Name);
        }

        protected virtual FileSystemItem CreateDirectoryUpFileSystemItem(IDirectoryInfo directoryInfo)
        {
            if (directoryInfo == null) return null;
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


        private IEnumerable<FileSystemItem> _items;

        public IEnumerable<FileSystemItem> Items
        {
            get => _items;
            private set => SetProperty(ref _items, value);
        }
    }
}
