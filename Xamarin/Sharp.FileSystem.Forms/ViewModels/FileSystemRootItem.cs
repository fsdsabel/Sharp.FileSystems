using Sharp.FileSystems.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sharp.FileSystem.Forms.ViewModels
{
    /// <summary>
    /// Searches for file systems, this is the root node
    /// </summary>
    public class FileSystemRootItem : FileSystemItemBase
    {
        private readonly IFileSystemDiscovery[] _fileSystemDiscoverers;
        private readonly List<IDisposable> _networkDiscoverySubscriptions = new List<IDisposable>();
        private readonly List<FileSystemItemBase> _items = new List<FileSystemItemBase>();

        public override string Name => "/";

        public FileSystemRootItem(params IFileSystemDiscovery[] fileSystemDiscoverers)
        {
            _fileSystemDiscoverers = fileSystemDiscoverers;
            StartDiscovery();
        }

        public override Task<IEnumerable<FileSystemItemBase>> EnumerateChildrenAsync()
        {
            return Task.FromResult<IEnumerable<FileSystemItemBase>>(_items);
        }

        protected virtual void StartDiscovery()
        {
            foreach (var discovery in _fileSystemDiscoverers)
            {
                _networkDiscoverySubscriptions.Add(discovery
                    .DiscoverRootDirectoriesContinuous()
                    .Subscribe(OnFileSystemDiscovered));
            }
        }

        protected virtual void StopDiscovery()
        {
            foreach (var subscription in _networkDiscoverySubscriptions)
            {
                subscription.Dispose();
            }
            _networkDiscoverySubscriptions.Clear();
        }     

        protected virtual void OnFileSystemDiscovered(IFileSystemDiscoveryResult result)
        {
            lock (_items)
            {
                var existing = _items.OfType<FileSystemItem>().FirstOrDefault(f => f.FileSystemDiscoveryResult.Equals(result));
                if (existing == null)
                {
                    _items.Add(CreateFileSystemItem(result));
                }
            }

            OnChildrenChanged();
        }

        protected virtual FileSystemItemBase CreateFileSystemItem(IFileSystemDiscoveryResult discoveryResult)
        {
            return new FileSystemItem(this, discoveryResult);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopDiscovery();
            }
            base.Dispose(disposing);
        }

    }
}
