using Sharp.FileSystem.Forms.Fonts;
using Sharp.FileSystems.Abstractions;

namespace Sharp.FileSystem.Forms.ViewModels
{
    class DirectoryUpItem : DirectoryItem
    {
        public DirectoryUpItem(IDirectoryInfo fileSystemInfo) : base(fileSystemInfo)
        {
        }

        public override string Name => "..";

        public override string Icon => IconFontSolid.LevelUpAlt;
    }
}