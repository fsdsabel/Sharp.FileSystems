using Sharp.FileSystem.Forms.ViewModels;
using Sharp.FileSystems.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sharp.FileSystem.Forms
{
    public interface IFileBrowserDirectoryAdapter
    {
        IEnumerable<FileSystemItem> Items { get; }

        Task OpenItemAsync(FileSystemItem fileSystemItem);

        Task SetDirectoryAsync(IDirectoryInfo directoryInfo);

        Task OnErrorAsync(Exception exception);
    }
}