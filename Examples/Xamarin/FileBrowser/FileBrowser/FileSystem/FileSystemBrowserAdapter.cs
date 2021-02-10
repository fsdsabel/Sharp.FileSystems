using Sharp.FileSystem.Smb.Discovery;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace FileBrowser.FileSystem
{
    public class FileSystemBrowserAdapter : IDisposable
    {
        private IDisposable _networkDiscoverySubscription;

        public FileSystemBrowserAdapter()
        {

        }

       // public ObservableCollection<>
    

        protected virtual void StartDiscovery()
        {
            _networkDiscoverySubscription = new SmbFileSystemDiscovery()
                .DiscoverRootDirectoriesContinuous()
                .Subscribe(f =>
            {
                
            });
        }

        protected virtual void StopDiscovery()
        {

        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                _networkDiscoverySubscription?.Dispose();
            }            
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
