using FileSystem.Smb.Internal;
using Sharp.FileSystems.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileSystem.Smb
{
    internal class SmbDirectory : IDirectory
    {
        public SmbDirectory(IFileSystem fileSystem) //: base(fileSystem)
        {
            FileSystem = fileSystem;
        }

        public IFileSystem FileSystem { get; }

        public IDirectoryInfo CreateDirectory(string path)
        {
            var dirInfo = FileSystem.DirectoryInfo.FromDirectoryName(path);
            dirInfo.Create();
            return dirInfo;
        }

        public void Delete(string path)
        {
            var dirInfo = FileSystem.DirectoryInfo.FromDirectoryName(path);
            dirInfo.Delete();
        }

        public void Delete(string path, bool recursive)
        {
            var dirInfo = FileSystem.DirectoryInfo.FromDirectoryName(path);
            dirInfo.Delete(recursive);
        }

        public IEnumerable<string> EnumerateDirectories(string path)
        {
            return FileSystem.DirectoryInfo.FromDirectoryName(path).EnumerateDirectories().Select(p => p.FullName);
        }

        public IEnumerable<string> EnumerateDirectories(string path, string searchPattern)
        {
            return FileSystem.DirectoryInfo.FromDirectoryName(path).EnumerateDirectories(searchPattern).Select(p => p.FullName);
        }

        public IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
        {
            return FileSystem.DirectoryInfo.FromDirectoryName(path).EnumerateDirectories(searchPattern, searchOption).Select(p => p.FullName);
        }

        public  IEnumerable<string> EnumerateFiles(string path)
        {
            return EnumerateFiles(path, "*");
        }

        public  IEnumerable<string> EnumerateFiles(string path, string searchPattern)
        {
            return EnumerateFiles(path, searchPattern, SearchOption.TopDirectoryOnly);
        }

        public  IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
        {
            return FileSystem.DirectoryInfo.FromDirectoryName(path).EnumerateFiles(searchPattern, searchOption).Select(p => p.FullName);
                     
        }

        public  IEnumerable<string> EnumerateFileSystemEntries(string path)
        {
            return EnumerateFileSystemEntries(path, "*");
        }

        public  IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern)
        {
            return EnumerateFileSystemEntries(path, searchPattern, SearchOption.TopDirectoryOnly);
        }

        public  IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, SearchOption searchOption)
        {
            return FileSystem.DirectoryInfo.FromDirectoryName(path).EnumerateFileSystemInfos(searchPattern, searchOption).Select(p => p.FullName);
        }

        public  bool Exists(string path)
        {
            var info = FileSystem.DirectoryInfo.FromDirectoryName(path);
            info.Refresh();
            return info.Exists;
        }

        public  DateTime GetCreationTime(string path)
        {
            return FileSystem.DirectoryInfo.FromDirectoryName(path).CreationTime;
        }

        public  DateTime GetCreationTimeUtc(string path)
        {
            return FileSystem.DirectoryInfo.FromDirectoryName(path).CreationTimeUtc;
        }

        public  string GetCurrentDirectory()
        {
            throw new NotSupportedException();
        }
        
        public  string[] GetDirectories(string path)
        {
            return GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
        }

        public  string[] GetDirectories(string path, string searchPattern)
        {
            return GetDirectories(path, searchPattern, SearchOption.TopDirectoryOnly);
        }

        public  string[] GetDirectories(string path, string searchPattern, SearchOption searchOption)
        {
            return EnumerateDirectories(path, searchPattern, searchOption).ToArray();
        }

        public string GetDirectoryRoot(string path)
        {
            return FileSystem.DirectoryInfo.FromDirectoryName(path).Root.FullName;
        }

        public  string[] GetFiles(string path)
        {
            return EnumerateFiles(path).ToArray();
        }

        public  string[] GetFiles(string path, string searchPattern)
        {
            return EnumerateFiles(path, searchPattern).ToArray();
        }

        public  string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            return EnumerateFiles(path, searchPattern, searchOption).ToArray();
        }

        public  string[] GetFileSystemEntries(string path)
        {
            return GetFileSystemEntries(path, "*");
        }

        public  string[] GetFileSystemEntries(string path, string searchPattern)
        {
            return EnumerateFileSystemEntries(path, searchPattern, SearchOption.TopDirectoryOnly).ToArray();
        }

        public  DateTime GetLastAccessTime(string path)
        {
            return FileSystem.DirectoryInfo.FromDirectoryName(path).LastAccessTime;
        }

        public  DateTime GetLastAccessTimeUtc(string path)
        {
            return FileSystem.DirectoryInfo.FromDirectoryName(path).LastAccessTimeUtc;
        }

        public  DateTime GetLastWriteTime(string path)
        {
            return FileSystem.DirectoryInfo.FromDirectoryName(path).LastWriteTime;
        }

        public  DateTime GetLastWriteTimeUtc(string path)
        {
            return FileSystem.DirectoryInfo.FromDirectoryName(path).LastWriteTimeUtc;
        }

        public  string[] GetLogicalDrives(string path)
        {
            using (var client = SmbClient.GetSmbClient(path))
            {
                var shares = client.Client.ListShares(out var status);
                if (status != SMBLibrary.NTStatus.STATUS_SUCCESS)
                {
                    throw new SmbIOException($"Failed to list shares: {status}", status);
                }
                return shares.ToArray();
            }
        }

        public  IDirectoryInfo GetParent(string path)
        {
            return FileSystem.DirectoryInfo.FromDirectoryName(path).Parent;
        }

        public  void Move(string sourceDirName, string destDirName)
        {
            FileSystem.DirectoryInfo.FromDirectoryName(sourceDirName).MoveTo(destDirName);
        }

        public  void SetCreationTime(string path, DateTime creationTime)
        {
            FileSystem.DirectoryInfo.FromDirectoryName(path).CreationTime = creationTime;
        }

        public  void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
        {
            FileSystem.DirectoryInfo.FromDirectoryName(path).CreationTimeUtc = creationTimeUtc;
        }

        public  void SetCurrentDirectory(string path)
        {
            throw new NotSupportedException();
        }

        public  void SetLastAccessTime(string path, DateTime lastAccessTime)
        {
            FileSystem.DirectoryInfo.FromDirectoryName(path).LastAccessTime = lastAccessTime;
        }

        public  void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
        {
            FileSystem.DirectoryInfo.FromDirectoryName(path).LastAccessTimeUtc = lastAccessTimeUtc;
        }

        public  void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            FileSystem.DirectoryInfo.FromDirectoryName(path).LastWriteTime = lastWriteTime;
        }

        public  void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
        {
            FileSystem.DirectoryInfo.FromDirectoryName(path).LastWriteTimeUtc = lastWriteTimeUtc;
        }
    }
}
