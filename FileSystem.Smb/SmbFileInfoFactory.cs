using Sharp.FileSystems.Abstractions;

namespace Sharp.FileSystem.Smb
{
    internal class SmbFileInfoFactory : IFileInfoFactory
    {
        private SmbFileSystem cifsFileSystem;

        public SmbFileInfoFactory(SmbFileSystem cifsFileSystem)
        {
            this.cifsFileSystem = cifsFileSystem;
        }

        public IFileInfo FromFileName(string fileName)
        {
            var finfo = new SmbFileInfo(cifsFileSystem, fileName);
            finfo.Refresh();
            return finfo;
        }
    }
}