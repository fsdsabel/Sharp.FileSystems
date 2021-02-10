using SMBLibrary;
using System;
using System.IO;

namespace Sharp.FileSystem.Smb.Internal
{
    class SmbStream : Stream
    {
        private readonly SmbClient _cifsClient;
        private readonly object _handle;
        private readonly FileAccess _fileAccess;

        public SmbStream(SmbClient cifsClient, object handle, FileAccess fileAccess)
        {
            _cifsClient = cifsClient;
            _handle = handle;
            _fileAccess = fileAccess;
        }

        public override bool CanRead => _fileAccess.HasFlag(FileAccess.Read);
        public override bool CanSeek => true;
        public override bool CanWrite => _fileAccess.HasFlag(FileAccess.Write);
        public override long Length
        {
            get
            {
                return _cifsClient.GetFileInformation(null, false, _handle).StandardInformation.EndOfFile;
            }
        }
        public override long Position { get; set; }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int toread = count;
            int read = 0;
            int maxReadSize = (int)_cifsClient.FileStore.MaxReadSize;
            while (toread > 0)
            {
                try
                {
                    var status = _cifsClient.FileStore.ReadFile(out var data, _handle, Position, Math.Min(toread, maxReadSize));

                    if (status == NTStatus.STATUS_END_OF_FILE)
                    {
                        return read;
                    }

                    _cifsClient.ThrowOnError(status);

                    Array.Copy(data, 0, buffer, offset + read, data.Length);
                    read += data.Length;
                    Position += data.Length;
                    toread -= data.Length;
                }
                catch (Exception e)
                {
                    if (!e.Message.Contains("credits")) throw e;
                }
            }
            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position += offset;
                    break;
                case SeekOrigin.End:
                    Position = Length - offset;
                    break;
            }
            return Position;
        }

        public override void SetLength(long value)
        {
            _cifsClient.ThrowOnError(_cifsClient.FileStore.SetFileInformation(_handle, new FileEndOfFileInformation { EndOfFile = value }));
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count <= _cifsClient.FileStore.MaxWriteSize && offset == 0 && count == buffer.Length)
            {
                _cifsClient.ThrowOnError(_cifsClient.FileStore.WriteFile(out var bytesWritten, _handle, Position, buffer));
                Position += count;
            }
            else
            {
                int towrite = count;
                int written = 0;
                while (towrite > 0)
                {
                    var tempbuffer = new byte[Math.Min(towrite, _cifsClient.FileStore.MaxWriteSize)];
                    Array.Copy(buffer, offset + written, tempbuffer, 0, tempbuffer.Length);

                    try
                    {
                        _cifsClient.ThrowOnError(_cifsClient.FileStore.WriteFile(out var bytesWritten, _handle, Position, tempbuffer));

                        Position += bytesWritten;
                        towrite -= bytesWritten;
                        written += bytesWritten;
                    }
                    catch (Exception e)
                    {
                        if (!e.Message.Contains("credits")) throw e;
                    }
                }
            }
        }

        public override void Close()
        {
            base.Close();
            _cifsClient.CloseFile(_handle);
            _cifsClient.Dispose();
        }
    }
}