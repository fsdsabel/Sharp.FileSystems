using Sharp.FileSystem.Forms.Fonts;
using Sharp.FileSystems.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sharp.FileSystem.Forms.ViewModels
{
    /// <summary>
    /// Represents a file system entry that lists all logical drives
    /// </summary>
    public class FileSystemItem : FileSystemItemBase
    {
        private readonly FileSystemRootItem _parent;

        public FileSystemItem(FileSystemRootItem parent, IFileSystemDiscoveryResult fileSystemDiscoveryResult)
        {
            _parent = parent;
            FileSystemDiscoveryResult = fileSystemDiscoveryResult;
        }

        public IFileSystemDiscoveryResult FileSystemDiscoveryResult { get; }

        public override string Name => FileSystemDiscoveryResult.DisplayName;

        public override string Icon => IconFontSolid.Folder;


        public override async Task<IEnumerable<FileSystemItemBase>> EnumerateChildrenAsync()
        {
            return await Task.Run(() =>
            {
                var fsdr = FileSystemDiscoveryResult;
                var drives = fsdr.FileSystem.Directory.GetLogicalDrives(fsdr.RootPath);
                var result = new List<FileSystemItemBase>();
                result.Add(new DirectoryUpItem(_parent));

                foreach (var drive in drives)
                {
                    var dirinfo = fsdr.FileSystem.DirectoryInfo.FromDirectoryName(fsdr.FileSystem.Path.Combine(fsdr.RootPath, drive), false);
                    result.Add(CreateFileSystemItem(dirinfo, this));
                }
                return result;
            }).ConfigureAwait(false);
        }        
    }
}
