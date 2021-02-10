using SMBLibrary;
using System.IO;

namespace Sharp.FileSystem.Smb
{
    public class SmbIOException : IOException
    {
        public SmbIOException(string message, NTStatus status) : base(message)
        {
            Status = status;
        }

        public NTStatus Status { get; }
    }
}
