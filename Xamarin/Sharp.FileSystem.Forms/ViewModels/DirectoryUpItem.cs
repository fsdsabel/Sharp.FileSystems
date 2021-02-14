using Sharp.FileSystem.Forms.Fonts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sharp.FileSystem.Forms.ViewModels
{
    /// <summary>
    /// Represents a directory up entry
    /// </summary>
    public class DirectoryUpItem : DirectoryItem
    {
        private readonly FileSystemItemBase _targetItem;
        
        public DirectoryUpItem(FileSystemItemBase targetItem) : base(null, null)
        {
            _targetItem = targetItem;
        }

        public override string Name => "..";

        public override string Icon => IconFontSolid.LevelUpAlt;

        public override Task<IEnumerable<FileSystemItemBase>> EnumerateChildrenAsync()
        {
            return _targetItem.EnumerateChildrenAsync();
        }
    }    

    
}