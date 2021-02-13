using Sharp.FileSystems.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace FileBrowser.FileSystem
{
    public interface IFileBrowserDirectoryAdapter
    {
        IEnumerable<FileSystemItem> Items { get; }

        Task OpenItemAsync(FileSystemItem fileSystemItem);

        Task SetDirectoryAsync(IDirectoryInfo directoryInfo);

        Task OnErrorAsync(Exception exception);
    }
}