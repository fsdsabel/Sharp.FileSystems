using Sharp.FileSystems.Abstractions;

namespace FileSystem.Smb
{
    internal class SmbDirectoryInfoFactory : IDirectoryInfoFactory
    {
        private readonly IFileSystem _fileSystem;

        public SmbDirectoryInfoFactory(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public IDirectoryInfo FromDirectoryName(string directoryName)
        {
            var info = new SmbDirectoryInfo(_fileSystem, directoryName);
            info.Refresh();
            return info;
        }
    }
}