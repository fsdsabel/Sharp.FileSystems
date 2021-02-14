using Sharp.FileSystem.Forms.ViewModels;
using Sharp.FileSystems.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sharp.FileSystem.Forms
{
    public interface IFileBrowserDirectoryAdapter
    {
        IEnumerable<FileSystemItemBase> Items { get; }

        Task OpenItemAsync(FileSystemItemBase fileSystemItem);

        Task RefreshAsync();

        Task OnErrorAsync(Exception exception);
    }
}