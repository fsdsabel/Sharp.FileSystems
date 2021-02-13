using Sharp.FileSystems.Abstractions;

namespace Sharp.FileSystem.Smb
{
    internal class SmbDirectoryInfoFactory : IDirectoryInfoFactory
    {
        private readonly IFileSystem _fileSystem;

        public SmbDirectoryInfoFactory(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public IDirectoryInfo FromDirectoryName(string directoryName, bool refresh)
        {
            var info = new SmbDirectoryInfo(_fileSystem, directoryName);
            if (refresh)
            {
                info.Refresh();
            }
            return info;
        }
    }
}