using Sharp.FileSystem.Forms.Fonts;
using Sharp.FileSystems.Abstractions;

namespace Sharp.FileSystem.Forms.ViewModels
{
    class DirectoryItem : FileSystemItem
    {
        public DirectoryItem(IDirectoryInfo dirInfo) : base(dirInfo)
        {
            DirInfo = dirInfo;
        }

        public IDirectoryInfo DirInfo { get; }

        public override string Icon => IconFontSolid.Folder;
    }
}