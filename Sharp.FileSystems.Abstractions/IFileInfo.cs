using System.IO;

namespace Sharp.FileSystems.Abstractions
{
    public interface IFileInfo : IFileSystemInfo
    {
        /// <inheritdoc cref="FileInfo.AppendText"/>
        StreamWriter AppendText();
        /// <inheritdoc cref="FileInfo.CopyTo(string)"/>
        IFileInfo CopyTo(string destFileName);
        /// <inheritdoc cref="FileInfo.CopyTo(string,bool)"/>
        IFileInfo CopyTo(string destFileName, bool overwrite);
        /// <inheritdoc cref="FileInfo.Create"/>
        Stream Create();
        /// <inheritdoc cref="FileInfo.CreateText"/>
        StreamWriter CreateText();
        /// <inheritdoc cref="FileInfo.Decrypt"/>
        void Decrypt();
        /// <inheritdoc cref="FileInfo.Encrypt"/>
        void Encrypt();
        /// <inheritdoc cref="FileInfo.MoveTo(string)"/>
        void MoveTo(string destFileName);
        /// <inheritdoc cref="FileInfo.Open(FileMode)"/>
        Stream Open(FileMode mode);
        /// <inheritdoc cref="FileInfo.Open(FileMode,FileAccess)"/>
        Stream Open(FileMode mode, FileAccess access);
        /// <inheritdoc cref="FileInfo.Open(FileMode,FileAccess,FileShare)"/>
        Stream Open(FileMode mode, FileAccess access, FileShare share);
        /// <inheritdoc cref="FileInfo.OpenRead"/>
        Stream OpenRead();
        /// <inheritdoc cref="FileInfo.OpenText"/>
        StreamReader OpenText();
        /// <inheritdoc cref="FileInfo.OpenWrite"/>
        Stream OpenWrite();
       
        /// <inheritdoc cref="FileInfo.Directory"/>
        IDirectoryInfo Directory { get; }
        /// <inheritdoc cref="FileInfo.DirectoryName"/>
        string DirectoryName { get; }
        /// <inheritdoc cref="FileInfo.IsReadOnly"/>
        bool IsReadOnly { get; set; }
        /// <inheritdoc cref="FileInfo.Length"/>
        long Length { get; }
    }
}
