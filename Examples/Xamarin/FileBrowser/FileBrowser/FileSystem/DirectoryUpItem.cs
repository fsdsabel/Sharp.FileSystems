using FileBrowser.Fonts;
using Sharp.FileSystems.Abstractions;

namespace FileBrowser.FileSystem
{
    class DirectoryUpItem : DirectoryItem
    {
        public DirectoryUpItem(IDirectoryInfo fileSystemInfo) : base(fileSystemInfo)
        {
        }

        public override string Name => "..";

        public override string Icon => IconFontSolid.LevelUpAlt;

        public override string IconFont => "FAS";
    }
}