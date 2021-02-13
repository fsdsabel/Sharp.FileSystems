using Sharp.FileSystem.Forms.Fonts;
using Sharp.FileSystems.Abstractions;

namespace Sharp.FileSystem.Forms.ViewModels
{
    public class FileSystemRootItem : FileSystemItem
    {
        public FileSystemRootItem(IFileSystemDiscoveryResult fileSystemDiscoveryResult) : base(null)
        {
            FileSystemDiscoveryResult = fileSystemDiscoveryResult;
        }

        public IFileSystemDiscoveryResult FileSystemDiscoveryResult { get; }

        public override string Name => FileSystemDiscoveryResult.DisplayName;

        public override string Icon => IconFontSolid.Folder;

    }
}
