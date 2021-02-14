using Sharp.FileSystem.Forms.Fonts;
using Sharp.FileSystems.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sharp.FileSystem.Forms.ViewModels
{
    /// <summary>
    /// This is a file entry.
    /// </summary>
    public class FileItem : FileSystemItemBase
    {
        public FileItem(IFileInfo fileInfo) 
        {
            FileInfo = fileInfo ?? throw new System.ArgumentNullException(nameof(fileInfo));
        }

        public IFileInfo FileInfo { get; }

        public override string Icon
        {
            get
            {
                switch (FileInfo.Extension.ToLower())
                {
                    case ".jpg":
                    case ".bmp":
                    case ".gif":
                    case ".png":
                    case ".tiff":
                        return IconFontSolid.FileImage;
                    case ".pdf":
                        return IconFontSolid.FilePdf;
                    case ".ppt":
                    case ".pptx":
                        return IconFontSolid.FilePowerpoint;
                    case ".xls":
                    case ".xlsx":
                        return IconFontSolid.FileExcel;
                    case ".doc":
                    case ".docx":
                        return IconFontSolid.FileWord;
                    case ".mp3":
                    case ".aac":
                        return IconFontSolid.FileAudio;
                    case ".zip":
                    case ".rar":
                    case ".7z":
                    case ".gz":
                    case ".bz2":
                        return IconFontSolid.FileArchive;
                    case ".avi":
                    case ".mp4":
                    case ".mkv":
                    case ".wmv":
                    case ".mov":
                        return IconFontSolid.FileVideo;
                }
                return IconFontSolid.File;
            }
        }

        public override string Name => FileInfo.Name;

        public override Task<IEnumerable<FileSystemItemBase>> EnumerateChildrenAsync()
        {
            return Task.FromResult(Enumerable.Empty<FileSystemItemBase>());
        }
    }
}