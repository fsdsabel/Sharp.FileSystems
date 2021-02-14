using Sharp.FileSystem.Smb.Internal;
using Sharp.FileSystems.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sharp.FileSystem.Smb
{
    internal class SmbDirectoryInfo : IDirectoryInfo
    {
        private readonly string _directoryName;
        private readonly Uri _uri;
        private readonly string _smbPath;
        private DateTime _creationTime;
        private DateTime _creationTimeUtc;
        private FileAttributes _attributes;
        private DateTime _lastAccessTime;
        private DateTime _lastAccessTimeUtc;
        private DateTime _lastWriteTime;
        private DateTime _lastWriteTimeUtc;        

        public SmbDirectoryInfo(IFileSystem fileSystem, string directoryName)
        {
            FileSystem = fileSystem;
            _directoryName = directoryName;
            _uri = new Uri(directoryName);
            FullName = Uri.UnescapeDataString(_uri.AbsolutePath);
            _smbPath = Uri.UnescapeDataString(string.Join("", _uri.Segments.Skip(2)));
            if (string.IsNullOrEmpty(FullName))
            {
                FullName = "/";
            }
        }


        private string ChangePathOfCurrentUri(string path)
        {
            var uri = new Uri(_directoryName);
            var sharename = SmbClient.SharenameFromUri(uri);
            var uriBuilder = new UriBuilder("smb", uri.Host);
            uriBuilder.UserName = uri.UserInfo?.Split(':').FirstOrDefault();
            uriBuilder.Password = uri.UserInfo?.Split(':').Skip(1).FirstOrDefault();
            uriBuilder.Path = Path.Combine(sharename, path).Replace('\\', '/');
            return uriBuilder.Uri.ToString();
        }

        public IDirectoryInfo Parent
        {
            get
            {
                var parts = _smbPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (parts.Count < 1) return null;
                parts.RemoveAt(parts.Count - 1);
                return FileSystem.DirectoryInfo.FromDirectoryName(ChangePathOfCurrentUri(string.Join("/", parts)), false);
            }
        }
        public IDirectoryInfo Root
        {
            get
            {
                return FileSystem.DirectoryInfo.FromDirectoryName(ChangePathOfCurrentUri(""), false);
            }
        }
        public IFileSystem FileSystem { get; }
        public FileAttributes Attributes
        {
            get => _attributes;
            set
            {
                using (var client = SmbClient.GetSmbClient(_directoryName))
                {
                    var fi = new SMBLibrary.FileBasicInformation();
                    fi.FileAttributes = SmbHelper.MapFileAttributes(value);
                    client.SetFileInformation(_smbPath, true, fi);
                }
                _attributes = value;
            }
        }

        private void SetTime(ref DateTime field, DateTime value, Action<SMBLibrary.FileBasicInformation> setter)
        {
            using (var client = SmbClient.GetSmbClient(_directoryName))
            {
                var fi = new SMBLibrary.FileBasicInformation();
                setter(fi);
                client.SetFileInformation(_smbPath, true, fi);
            }
            field = value;
        }


        public DateTime CreationTime
        {
            get => _creationTime;
            set => SetTime(ref _creationTime, value, fi => fi.CreationTime = value.ToUniversalTime());
        }
        public DateTime CreationTimeUtc
        {
            get => _creationTimeUtc;
            set => SetTime(ref _creationTime, value, fi => fi.CreationTime = value);
        }
        public bool Exists { get; private set; }
        public string Extension => Path.GetExtension(FullName);
        public string FullName { get; private set; }
        public DateTime LastAccessTime
        {
            get => _lastAccessTime;
            set => SetTime(ref _lastAccessTime, value, fi => fi.LastAccessTime = value.ToUniversalTime());
        }
        public DateTime LastAccessTimeUtc
        {
            get => _lastAccessTimeUtc;
            set => SetTime(ref _lastAccessTime, value, fi => fi.LastAccessTime = value);
        }
        public DateTime LastWriteTime
        {
            get => _lastWriteTime;
            set => SetTime(ref _lastWriteTime, value, fi => fi.LastWriteTime = value.ToUniversalTime());
        }
        public DateTime LastWriteTimeUtc
        {
            get => _lastWriteTimeUtc;
            set => SetTime(ref _lastWriteTime, value, fi => fi.LastWriteTime = value);
        }
        public string Name => Path.GetFileName(FullName);

        public Uri Uri => _uri;

        private SmbDirectoryInfo CreateChildDirectoryInfo(string smbPath)
        {
            var builder = new UriBuilder(_uri);
            // keep share (first part)
            builder.Path = Path.Combine(string.Join("", _uri.Segments.Take(2)), smbPath);
            return new SmbDirectoryInfo(FileSystem, builder.Uri.ToString());
        }

        private SmbFileInfo CreateChildFileInfo(string smbPath)
        {
            var builder = new UriBuilder(_uri);
            // keep share (first part)            
            builder.Path = Path.Combine(string.Join("", _uri.Segments.Take(2)), smbPath);
            return new SmbFileInfo(FileSystem, builder.Uri.ToString());
        }

        public void Create()
        {
            using (var client = SmbClient.GetSmbClient(_directoryName))
            {
                client.CreateDirectory(_smbPath);
                Refresh(client);
            }
        }

        public IDirectoryInfo CreateSubdirectory(string path)
        {
            using (var client = SmbClient.GetSmbClient(_directoryName))
            {
                var fullpath = Path.Combine(_smbPath, path);
                client.CreateDirectory(fullpath);
                return FileSystem.DirectoryInfo.FromDirectoryName(ChangePathOfCurrentUri(fullpath));
            }
        }

        public void Delete(bool recursive)
        {
            using (var client = SmbClient.GetSmbClient(_directoryName))
            {
                try
                {
                    client.DeleteFile(_smbPath, true, recursive);
                }
                catch (SmbIOException ex) when (ex.Status == SMBLibrary.NTStatus.STATUS_OBJECT_NAME_NOT_FOUND) { }
            }
        }

        public void Delete()
        {
            Delete(false);
        }

        public IEnumerable<IDirectoryInfo> EnumerateDirectories()
        {
            return EnumerateDirectories("*");
        }

        public IEnumerable<IDirectoryInfo> EnumerateDirectories(string searchPattern)
        {
            return EnumerateDirectories(searchPattern, SearchOption.TopDirectoryOnly);
        }

        public IEnumerable<IDirectoryInfo> EnumerateDirectories(string searchPattern, SearchOption searchOption)
        {
            using (var client = SmbClient.GetSmbClient(_directoryName))
            {
                return client.EnumerateFileEntries(_smbPath, searchPattern, searchOption)
                    .Select(f => CreateChildDirectoryInfo(f.FileName));
            }
        }

        public IEnumerable<IFileInfo> EnumerateFiles()
        {
            return EnumerateFiles("*");
        }

        public IEnumerable<IFileInfo> EnumerateFiles(string searchPattern)
        {
            return EnumerateFiles(searchPattern, SearchOption.TopDirectoryOnly);
        }

        public IEnumerable<IFileInfo> EnumerateFiles(string searchPattern, SearchOption searchOption)
        {
            using (var client = SmbClient.GetSmbClient(_directoryName))
            {
                return client.EnumerateFileEntries(_smbPath, searchPattern, searchOption)
                    .Where(f => !f.FileAttributes.HasFlag(SMBLibrary.FileAttributes.Directory))
                    .Select(f => CreateChildFileInfo(f.FileName));
            }
        }

        public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos()
        {
            return EnumerateFileSystemInfos("*");
        }

        public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(string searchPattern)
        {
            return EnumerateFileSystemInfos(searchPattern, SearchOption.TopDirectoryOnly);
        }

        public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(string searchPattern, SearchOption searchOption)
        {
            using (var client = SmbClient.GetSmbClient(_directoryName))
            {
                return client.EnumerateFileEntries(_smbPath, searchPattern, searchOption)
                    .Select(f =>
                    {
                        if (f.FileAttributes.HasFlag(SMBLibrary.FileAttributes.Directory))
                        {
                            return (IFileSystemInfo)CreateChildDirectoryInfo(f.FileName);
                        }
                        return CreateChildFileInfo(f.FileName);
                    });
            }
        }

        public IDirectoryInfo[] GetDirectories()
        {
            return GetDirectories("*");
        }

        public IDirectoryInfo[] GetDirectories(string searchPattern)
        {
            return GetDirectories(searchPattern, SearchOption.TopDirectoryOnly);
        }

        public IDirectoryInfo[] GetDirectories(string searchPattern, SearchOption searchOption)
        {
            return EnumerateDirectories(searchPattern, searchOption).ToArray();
        }

        public IFileInfo[] GetFiles()
        {
            return GetFiles("*");
        }

        public IFileInfo[] GetFiles(string searchPattern)
        {
            return GetFiles(searchPattern, SearchOption.TopDirectoryOnly);
        }

        public IFileInfo[] GetFiles(string searchPattern, SearchOption searchOption)
        {
            return EnumerateFiles(searchPattern, searchOption).ToArray();
        }

        public IFileSystemInfo[] GetFileSystemInfos()
        {
            return GetFileSystemInfos("*");
        }

        public IFileSystemInfo[] GetFileSystemInfos(string searchPattern)
        {
            return GetFileSystemInfos(searchPattern, SearchOption.TopDirectoryOnly);
        }

        public IFileSystemInfo[] GetFileSystemInfos(string searchPattern, SearchOption searchOption)
        {
            return EnumerateFileSystemInfos(searchPattern, searchOption).ToArray();
        }

        public void MoveTo(string destDirName)
        {
            using (var client = SmbClient.GetSmbClient(_directoryName))
            {
                var handle = client.OpenFile(_smbPath, true, FileMode.Open, FileAccess.ReadWrite);
                try
                {
                    var destInfo = new SmbDirectoryInfo(FileSystem, destDirName);

                    var fdi = new SMBLibrary.FileRenameInformationType2 { FileName = destInfo._smbPath };

                    client.ThrowOnError(client.FileStore.SetFileInformation(handle, fdi));
                }
                finally
                {
                    client.CloseFile(handle);
                }
            }
        }

        public void Refresh()
        {
            using (var client = SmbClient.GetSmbClient(_directoryName))
            {
                Refresh(client);
            }
        }

        private void Refresh(SmbClient client)
        {
            try
            {
                var info = client.GetFileInformation(_smbPath, true);
                Exists = true;
                _lastAccessTime = info.BasicInformation.LastAccessTime.Time.GetValueOrDefault().ToLocalTime();
                _lastAccessTimeUtc = info.BasicInformation.LastAccessTime.Time.GetValueOrDefault();
                _lastWriteTime = info.BasicInformation.LastWriteTime.Time.GetValueOrDefault().ToLocalTime();
                _lastWriteTimeUtc = info.BasicInformation.LastWriteTime.Time.GetValueOrDefault();
                _creationTime = info.BasicInformation.CreationTime.Time.GetValueOrDefault().ToLocalTime();
                _creationTimeUtc = info.BasicInformation.CreationTime.Time.GetValueOrDefault();
                _attributes = SmbHelper.MapFileAttributes(info.BasicInformation.FileAttributes);
            }
            catch
            {
                Exists = false;
            }
        }
    }
}