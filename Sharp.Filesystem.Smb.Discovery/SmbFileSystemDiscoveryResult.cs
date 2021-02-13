using Sharp.FileSystems.Abstractions;
using System.Collections.Generic;

namespace Sharp.FileSystem.Smb.Discovery
{
    class SmbFileSystemDiscoveryResult : IFileSystemDiscoveryResult
    {
        public SmbFileSystemDiscoveryResult(IFileSystem fileSystem, string rootPath, string displayName)
        {
            FileSystem = fileSystem;
            RootPath = rootPath;
            DisplayName = displayName;
        }

        public string DisplayName { get; }
        public IFileSystem FileSystem { get;  }

        public string RootPath { get; }

        public override bool Equals(object obj)
        {
            if(obj is SmbFileSystemDiscoveryResult smbResult)
            {
                // RootPath might be different depending on if we got ip or dns name
                return smbResult.DisplayName == DisplayName;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return 1862586150 + EqualityComparer<string>.Default.GetHashCode(DisplayName);
        }
    }
}
