using Sharp.FileSystem.Forms.Fonts;
using Sharp.FileSystems.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sharp.FileSystem.Forms.ViewModels
{
    /// <summary>
    /// Represents a standard directory entry.
    /// </summary>
    public class DirectoryItem : FileSystemItemBase
    {
        private readonly FileSystemItemBase _parent;

        public DirectoryItem(FileSystemItemBase parent, IDirectoryInfo dirInfo) 
        {
            _parent = parent;
            DirInfo = dirInfo;
        }

        public virtual IDirectoryInfo DirInfo { get; }

        public override string Icon => IconFontSolid.Folder;

        public override string Name => DirInfo.Name;

        public override async Task<IEnumerable<FileSystemItemBase>> EnumerateChildrenAsync()
        {           
            return await Task.Run(() =>
            {
                var items = new List<FileSystemItemBase>();
                items.Add(new DirectoryUpItem(_parent));
                foreach (var item in DirInfo.EnumerateFileSystemInfos())
                {
                    var vitem = CreateFileSystemItem(item, this);
                    if (vitem != null)
                    {
                        items.Add(vitem);
                    }
                }
                return items;
            }).ConfigureAwait(false);
        }
    }
}