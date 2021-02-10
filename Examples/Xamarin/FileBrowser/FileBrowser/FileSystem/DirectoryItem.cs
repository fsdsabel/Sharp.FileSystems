using FileBrowser.Fonts;
using Sharp.FileSystems.Abstractions;

namespace FileBrowser.FileSystem
{
    class DirectoryItem : FileSystemItem
    {
        public DirectoryItem(IDirectoryInfo dirInfo) : base(dirInfo)
        {
            DirInfo = dirInfo;
        }

        public IDirectoryInfo DirInfo { get; }

        public override string Icon => IconFontRegular.Folder;
    }
}