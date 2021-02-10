using FileBrowser.Fonts;
using Sharp.FileSystems.Abstractions;

namespace FileBrowser.FileSystem
{
    class FileItem : FileSystemItem
    {
        public FileItem(IFileInfo fileInfo) : base(fileInfo)
        {
            FileInfo = fileInfo;
        }

        public IFileInfo FileInfo { get; }

        public override string Icon
        {
            get
            {
                switch(FileInfo.Extension.ToLower())
                {
                    case ".jpg":
                    case ".bmp":
                    case ".gif":
                    case ".png":
                    case ".tiff":
                        return IconFontRegular.FileImage;
                    case ".pdf":
                        return IconFontRegular.FilePdf;
                    case ".ppt":
                    case ".pptx":
                        return IconFontRegular.FilePowerpoint;
                    case ".xls":
                    case ".xlsx":
                        return IconFontRegular.FileExcel;
                    case ".doc":
                    case ".docx":
                        return IconFontRegular.FileWord;
                    case ".mp3":
                    case ".aac":
                        return IconFontRegular.FileAudio;
                    case ".zip":
                    case ".rar":
                    case ".7z":
                    case ".gz":
                    case ".bz2":
                        return IconFontRegular.FileArchive;
                    case ".avi":
                    case ".mp4":
                    case ".mkv":
                    case ".wmv":
                    case ".mov":
                        return IconFontRegular.FileVideo;
                }
                return IconFontRegular.File;
            }
        }
    }
}