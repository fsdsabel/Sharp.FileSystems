using Sharp.FileSystems.Abstractions;

namespace Sharp.FileSystem.Forms.ViewModels
{
    public abstract class FileSystemItem
    {
        private readonly IFileSystemInfo _fileSystemInfo;

        protected FileSystemItem(IFileSystemInfo fileSystemInfo)
        {
            _fileSystemInfo = fileSystemInfo;
        }

        public virtual string Name => _fileSystemInfo.Name;

        public virtual string Icon { get; }

        public virtual string IconFont => "FAS";
    }
}