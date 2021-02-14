using Sharp.FileSystem.Smb.Internal;
using Sharp.FileSystems.Abstractions;
using System;
using System.IO;
using System.Linq;

namespace Sharp.FileSystem.Smb
{

    internal class SmbFileInfo : IFileInfo
    {
        private readonly string _fileName;
        private readonly Uri _uri;
        private readonly string _smbPath;
        private FileAttributes _attributes;
        private DateTime _creationTime;
        private DateTime _creationTimeUtc;
        private DateTime _lastAccessTime;
        private DateTime _lastAccessTimeUtc;
        private DateTime _lastWriteTime;
        private DateTime _lastWriteTimeUtc;

        public SmbFileInfo(IFileSystem fileSystem, string fileName)
        {
            FileSystem = fileSystem;
            _fileName = fileName;
            _uri = new Uri(fileName);
            _smbPath = Uri.UnescapeDataString(string.Join("", _uri.Segments.Skip(2)));
            FullName =  Uri.UnescapeDataString(_uri.AbsolutePath);
        }

        public IDirectoryInfo Directory { get; }
        public string DirectoryName { get; }
        public bool IsReadOnly { get; set; }
        public long Length { get; }
        public IFileSystem FileSystem { get; }
        public FileAttributes Attributes
        {
            get => _attributes;
            set
            {
                using (var client = SmbClient.GetSmbClient(_fileName))
                {
                    var fi = new SMBLibrary.FileBasicInformation();
                    fi.FileAttributes = SmbHelper.MapFileAttributes(value);
                    client.SetFileInformation(_smbPath, false, fi);
                }
                _attributes = value;
            }
        }

        private void SetTime(ref DateTime field, DateTime value, Action<SMBLibrary.FileBasicInformation> setter)
        {
            using (var client = SmbClient.GetSmbClient(_fileName))
            {
                var fi = new SMBLibrary.FileBasicInformation();
                setter(fi);
                client.SetFileInformation(_smbPath, false, fi);
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
        public bool Exists
        {
            get; private set;
        }
        public string Extension => Path.GetExtension(FullName);
        public string FullName { get; }
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

        public StreamWriter AppendText()
        {
            var stream = Open(FileMode.Append, FileAccess.Write);
            stream.Position = stream.Length;
            return new StreamWriter(stream);
        }

        public IFileInfo CopyTo(string destFileName)
        {
            return CopyTo(destFileName, false);
        }

        public IFileInfo CopyTo(string destFileName, bool overwrite)
        {
            using (var source = OpenRead())
            {
                var destFile = FileSystem.FileInfo.FromFileName(destFileName);
                using (var dest = FileSystem.FileInfo.FromFileName(destFileName).Open(overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write))
                {
                    source.CopyTo(dest);
                }
                return destFile;
            }
        }

        public Stream Create()
        {
            var client = SmbClient.GetSmbClient(_fileName);
            return new SmbStream(client, client.OpenFile(_smbPath, false, FileMode.Create, FileAccess.ReadWrite), FileAccess.ReadWrite);
        }

        public StreamWriter CreateText()
        {
            return new StreamWriter(Open(FileMode.Create, FileAccess.Write));
        }

        public void Decrypt()
        {
            throw new NotSupportedException();
        }

        public void Delete()
        {
            using (var client = SmbClient.GetSmbClient(_fileName))
            {
                try
                {
                    client.DeleteFile(_smbPath, false, false);
                }
                catch (SmbIOException ex) when (ex.Status == SMBLibrary.NTStatus.STATUS_OBJECT_NAME_NOT_FOUND) { }
            }
        }

        public void Encrypt()
        {
            throw new NotSupportedException();
        }

        public void MoveTo(string destFileName)
        {
            using (var client = SmbClient.GetSmbClient(_fileName))
            {
                var handle = client.OpenFile(_smbPath, false, FileMode.Open, FileAccess.ReadWrite);
                try
                {
                    var destInfo = new SmbFileInfo(FileSystem, destFileName);

                    var fdi = new SMBLibrary.FileRenameInformationType2
                    {
                        FileName = destInfo._smbPath,
                        ReplaceIfExists = true
                    };

                    client.ThrowOnError(client.FileStore.SetFileInformation(handle, fdi));
                }
                finally
                {
                    client.CloseFile(handle);
                }
            }
        }

        public Stream Open(FileMode mode)
        {
            return Open(mode, FileAccess.ReadWrite);
        }

        public Stream Open(FileMode mode, FileAccess access)
        {
            return Open(mode, access, FileShare.None);
        }

        public Stream Open(FileMode mode, FileAccess access, FileShare share)
        {
            var client = SmbClient.GetSmbClient(_fileName);
            return new BufferedStream(new SmbStream(client, client.OpenFile(_smbPath, false, mode, access, share), access));
        }

        public Stream OpenRead()
        {
            return Open(FileMode.Open, FileAccess.Read);
        }

        public StreamReader OpenText()
        {
            return new StreamReader(OpenRead());
        }

        public Stream OpenWrite()
        {
            return Open(FileMode.Open, FileAccess.Write);
        }

        public void Refresh()
        {
            using (var client = SmbClient.GetSmbClient(_fileName))
            {
                Refresh(client);
            }
        }

        private void Refresh(SmbClient client)
        {
            var result = client.FileStore.CreateFile(out var handle,
                           out var status,
                           _smbPath,
                           SMBLibrary.AccessMask.GENERIC_READ,
                           SMBLibrary.FileAttributes.Normal,
                           SMBLibrary.ShareAccess.Read | SMBLibrary.ShareAccess.Write,
                           SMBLibrary.CreateDisposition.FILE_OPEN,
                           SMBLibrary.CreateOptions.FILE_NON_DIRECTORY_FILE,
                           null);
            if (result == SMBLibrary.NTStatus.STATUS_SUCCESS)
            {
                client.CloseFile(handle);

                var info = client.GetFileInformation(_smbPath, false);
                _lastAccessTime = info.BasicInformation.LastAccessTime.Time.GetValueOrDefault().ToLocalTime();
                _lastAccessTimeUtc = info.BasicInformation.LastAccessTime.Time.GetValueOrDefault();
                _lastWriteTime = info.BasicInformation.LastWriteTime.Time.GetValueOrDefault().ToLocalTime();
                _lastWriteTimeUtc = info.BasicInformation.LastWriteTime.Time.GetValueOrDefault();
                _creationTime = info.BasicInformation.CreationTime.Time.GetValueOrDefault().ToLocalTime();
                _creationTimeUtc = info.BasicInformation.CreationTime.Time.GetValueOrDefault();
                _attributes = SmbHelper.MapFileAttributes(info.BasicInformation.FileAttributes);

                Exists = true;
            }
            else
            {
                Exists = false;
                _lastAccessTime = _lastAccessTimeUtc = _lastWriteTime = _lastWriteTimeUtc = _creationTime = _creationTimeUtc = default;
                _attributes = FileAttributes.Normal;
            }

        }
    }
}