using SMBLibrary;
using SMBLibrary.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Sharp.FileSystem.Smb.Internal
{



    class SmbClient : IDisposable
    {
        private string _connectionKey;


        protected SmbClient(string path)
        {
            _connectionKey = MakeKey(path);
            Client = new SMB2Client();
            var uri = new Uri(path);
            IPAddress.TryParse(uri.Host, out var ip);
            if(ip?.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
            {
                ip = null;
            }

            int retry = 0;
            while (ip == null && retry < 5)
            {
                var host = Dns.GetHostEntry(uri.Host);
                ip = host.AddressList.FirstOrDefault(i => i.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                retry++;
            }
            Sharename = SharenameFromUri(uri);
            if (!Client.Connect(ip, SMBTransportType.DirectTCPTransport))
            {
                throw new IOException("Cannot connect to SMB host.");
            }

            ThrowOnError(Client.Login("", "", ""));
            if (!string.IsNullOrEmpty(Sharename))
            {
                FileStore = Client.TreeConnect(Sharename, out var status);
                ThrowOnError(status);
            }
        }

        public static string SharenameFromUri(Uri uri)
        {
            return uri.AbsolutePath.Split('/').Skip(1).FirstOrDefault();
        }

        private static int _poolVersion;

        public static SmbClient GetSmbClient(string path)
        {

            var client = SmbConnectionPool.Remove(MakeKey(path), 5, out _poolVersion);
            if (client == null)
            {
                client = new SmbClient(path);
            }
            return client;
        }

        private static string MakeKey(string path)
        {
            var uri = new Uri(path);
            var sharename = uri.AbsolutePath.Split('/').Skip(1).FirstOrDefault();
            return uri.Host + "|" + sharename;
        }

        public SMB2Client Client { get; }

        public string Sharename { get; }
        public ISMBFileStore FileStore { get; }

        public IEnumerable<FileDirectoryInformation> EnumerateFileEntries(string path, string searchPattern, SearchOption searchOption)
        {
            if (path.StartsWith("/")) path = path.Substring(1);

            ThrowOnError(FileStore.CreateFile(
                   out var directoryHandle,
                   out var fileStatus,
                   path,
                   AccessMask.GENERIC_READ,
                   SMBLibrary.FileAttributes.Directory,
                   ShareAccess.Read | ShareAccess.Write,
                   CreateDisposition.FILE_OPEN,
                   CreateOptions.FILE_DIRECTORY_FILE,
                   null));

            List<QueryDirectoryFileInformation> fileList;
            var status = FileStore.QueryDirectory(out fileList, directoryHandle, searchPattern, FileInformationClass.FileDirectoryInformation);

            var result = fileList
                .OfType<FileDirectoryInformation>()
                .Where(f => f.FileName != "." && f.FileName != "..")
                .Select(f =>
                {
                    f.FileName = Path.Combine(path, f.FileName);
                    return f;
                });

            if (searchOption == SearchOption.AllDirectories)
            {
                FileStore.QueryDirectory(out var dirList, directoryHandle, "*", FileInformationClass.FileDirectoryInformation);
                var dirsToEnter = dirList.OfType<FileDirectoryInformation>()
                                        .Where(f => f.FileName != "." && f.FileName != "..")
                                        .Select(f =>
                                        {
                                            f.FileName = Path.Combine(path, f.FileName);
                                            return f;
                                        });

                var lresult = new List<FileDirectoryInformation>(result);
                foreach (var dir in dirsToEnter.Where(r => r.FileAttributes.HasFlag(SMBLibrary.FileAttributes.Directory)).ToArray())
                {
                    lresult.AddRange(EnumerateFileEntries(dir.FileName, searchPattern, searchOption));
                }
                ThrowOnError(FileStore.CloseFile(directoryHandle));
                return lresult;
            }

            ThrowOnError(FileStore.CloseFile(directoryHandle));
            return result;
        }

        public void CreateDirectory(string directoryName)
        {
            object directoryHandle = null;
            FileStatus fileStatus;

            string currentDir = "";
            foreach (var part in directoryName.Split('/'))
            {
                currentDir = Path.Combine(currentDir, part);
                ThrowOnError(FileStore.CreateFile(
                    out directoryHandle,
                    out fileStatus,
                    currentDir,
                    AccessMask.GENERIC_WRITE | AccessMask.SYNCHRONIZE,
                    SMBLibrary.FileAttributes.Directory,
                    ShareAccess.Delete | ShareAccess.Read | ShareAccess.Write,
                    CreateDisposition.FILE_OPEN_IF,
                    CreateOptions.FILE_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT,
                    null));
                if (fileStatus != FileStatus.FILE_CREATED && fileStatus != FileStatus.FILE_OPENED)
                {
                    throw new IOException($"Cannot create directory {directoryName}");
                }
                CloseFile(directoryHandle);
            }
        }

        public FileAllInformation GetFileInformation(string filename, bool isDirectory, object handle = null)
        {
            bool didCreateHandle = handle == null;
            if (handle == null)
            {
                handle = OpenFile(filename, isDirectory, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }
            try
            {
                ThrowOnError(FileStore.GetFileInformation(out var fileInfo, handle, FileInformationClass.FileAllInformation));

                return (FileAllInformation)fileInfo;
            }
            finally
            {
                if (didCreateHandle)
                {
                    CloseFile(handle);
                }
            }
        }

        public void SetFileInformation(string filename, bool isDirectory, FileInformation fileInformation, object handle = null)
        {
            bool didCreateHandle = handle == null;
            if (handle == null)
            {
                handle = OpenFile(filename, isDirectory, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            }
            try
            {
                ThrowOnError(FileStore.SetFileInformation(handle, fileInformation));
            }
            finally
            {
                if (didCreateHandle)
                {
                    CloseFile(handle);
                }
            }
        }

        public object OpenFile(
            string filename,
            bool isDirectory,
            FileMode fileMode = FileMode.Open,
            FileAccess fileAccess = FileAccess.Read,
            FileShare fileShare = FileShare.None,
            bool forDeletion = false)
        {
            AccessMask accessMask = 0;
            if (fileAccess == FileAccess.Read) accessMask |= AccessMask.GENERIC_READ;
            if (fileAccess == FileAccess.Write) accessMask |= AccessMask.GENERIC_WRITE;
            if (fileAccess == FileAccess.ReadWrite) accessMask |= AccessMask.GENERIC_WRITE | AccessMask.GENERIC_READ;

            if (forDeletion) accessMask |= AccessMask.DELETE;

            ShareAccess shareAccess = ShareAccess.None;
            if (fileShare == FileShare.Read) shareAccess |= ShareAccess.Read;
            if (fileShare == FileShare.Delete) shareAccess |= ShareAccess.Delete;
            if (fileShare == FileShare.Write) shareAccess |= ShareAccess.Write;
            if (fileShare == FileShare.ReadWrite) shareAccess |= ShareAccess.Write | ShareAccess.Read;

            CreateDisposition createDisposition = CreateDisposition.FILE_SUPERSEDE;
            if (fileMode == FileMode.CreateNew) createDisposition = CreateDisposition.FILE_CREATE;
            if (fileMode == FileMode.Create) createDisposition = CreateDisposition.FILE_SUPERSEDE;
            if (fileMode == FileMode.Open) createDisposition = CreateDisposition.FILE_OPEN;
            if (fileMode == FileMode.OpenOrCreate) createDisposition = CreateDisposition.FILE_OPEN_IF;
            if (fileMode == FileMode.Truncate) createDisposition = CreateDisposition.FILE_OVERWRITE_IF;
            if (fileMode == FileMode.Append) createDisposition = CreateDisposition.FILE_OPEN;

            CreateOptions createOptions = 0;
            if (isDirectory)
            {
                createOptions |= CreateOptions.FILE_DIRECTORY_FILE;
            }
            else
            {
                createOptions |= CreateOptions.FILE_NON_DIRECTORY_FILE;
            }


            object fileHandle;
            FileStatus fileStatus;
            ThrowOnError(FileStore.CreateFile(
                out fileHandle,
                out fileStatus,
                filename,
                accessMask,
                SMBLibrary.FileAttributes.Normal,
                shareAccess,
                createDisposition,
                createOptions,
                null));

            return fileHandle;
        }

        public void DeleteFile(string fileName, bool isDirectory, bool recursive)
        {
            if (recursive && isDirectory)
            {
                foreach (var item in EnumerateFileEntries(fileName, "*", SearchOption.TopDirectoryOnly))
                {
                    DeleteFile(item.FileName, item.FileAttributes.HasFlag(SMBLibrary.FileAttributes.Directory), true);
                }
            }

            var handle = OpenFile(fileName, isDirectory, FileMode.Open, FileAccess.ReadWrite, FileShare.None, true);
            FileDispositionInformation fileDispositionInformation = new FileDispositionInformation();
            fileDispositionInformation.DeletePending = true;
            ThrowOnError(FileStore.SetFileInformation(handle, fileDispositionInformation));
            ThrowOnError(FileStore.CloseFile(handle));
        }

        public void CloseFile(object handle)
        {
            ThrowOnError(FileStore.CloseFile(handle));
        }

        public void ThrowOnError(NTStatus status)
        {
            if (status != NTStatus.STATUS_SUCCESS && status != NTStatus.STATUS_PENDING)
            {
                switch(status)
                {
                    case NTStatus.STATUS_ACCESS_DENIED:
                        throw new SmbIOException($"Access denied.", status);                    
                }
                throw new SmbIOException($"Error using SMB, error code {status}", status);
            }
        }

        public bool IsClosed { get; private set; }

        public void Close()
        {
            try
            {
                FileStore.Disconnect();
                Client.Disconnect();
            }
            catch { }
            IsClosed = true;
        }

        protected virtual void Dispose(bool disposing)
        {
            SmbConnectionPool.Add(_connectionKey, this, _poolVersion);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~SmbClient()
        {
            Dispose(false);
        }
    }
}
