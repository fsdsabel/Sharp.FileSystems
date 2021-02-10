using Sharp.FileSystems.Abstractions;

namespace Sharp.FileSystem.Smb
{
    public class SmbFileSystem : IFileSystem
    {
        public SmbFileSystem()
        {
            Directory = new SmbDirectory(this);
            File = new SmbFile(this);
            DirectoryInfo = new SmbDirectoryInfoFactory(this);
            FileInfo = new SmbFileInfoFactory(this);
        }

        public IFile File { get; }
        public IDirectory Directory { get; }
        public IFileInfoFactory FileInfo { get; }
        public IFileStreamFactory FileStream { get; }
        public IPath Path { get; }
        public IDirectoryInfoFactory DirectoryInfo { get; }
        public IDriveInfoFactory DriveInfo { get; }
        public IFileSystemWatcherFactory FileSystemWatcher { get; }
    }
}
