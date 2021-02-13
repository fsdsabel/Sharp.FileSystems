using Sharp.FileSystems.Abstractions;

namespace FileBrowser.FileSystem
{
    /*class FileSystemDriveItem : FileSystemItem
    {
        public FileSystemDriveItem(IFileSystemInfo fileSystemInfo) : base(fileSystemInfo)
        {
        }
    }*/

    public class FileSystemRootItem : FileSystemItem
    {
        public FileSystemRootItem(IFileSystemDiscoveryResult fileSystemDiscoveryResult) : base(null)
        {
            FileSystemDiscoveryResult = fileSystemDiscoveryResult;
        }

        public IFileSystemDiscoveryResult FileSystemDiscoveryResult { get; }

        public override string Name => FileSystemDiscoveryResult.DisplayName;
    }
}
