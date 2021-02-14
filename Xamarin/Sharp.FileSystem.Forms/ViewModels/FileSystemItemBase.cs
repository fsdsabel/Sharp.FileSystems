using Sharp.FileSystems.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sharp.FileSystem.Forms.ViewModels
{
    public abstract class FileSystemItemBase : IDisposable
    {        
        private bool _disposedValue;

        public event EventHandler ChildrenChanged;

        protected FileSystemItemBase()
        {
        }

        public abstract string Name { get; }

        public virtual string Icon { get; }

        public virtual string IconFont => "FAS";

        public abstract Task<IEnumerable<FileSystemItemBase>> EnumerateChildrenAsync();

        protected virtual FileSystemItemBase CreateFileSystemItem(IFileSystemInfo fileSystemInfo, FileSystemItemBase parentItem)
        {
            if (fileSystemInfo is IDirectoryInfo directoryInfo)
            {
                return new DirectoryItem(parentItem, directoryInfo);
            }
            if (fileSystemInfo is IFileInfo fileInfo)
            {
                return new FileItem(fileInfo);
            }
            throw new NotSupportedException();
        }

        protected virtual void OnChildrenChanged()
        {
            ChildrenChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~FileSystemItemBase()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}