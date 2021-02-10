namespace Sharp.FileSystem.Smb.Internal
{
    static class SmbHelper
    {
        public static System.IO.FileAttributes MapFileAttributes(SMBLibrary.FileAttributes smbFileAttributes)
        {
            System.IO.FileAttributes result = 0;
            if (smbFileAttributes.HasFlag(SMBLibrary.FileAttributes.System)) result |= System.IO.FileAttributes.System;
            if (smbFileAttributes.HasFlag(SMBLibrary.FileAttributes.Archive)) result |= System.IO.FileAttributes.Archive;
            if (smbFileAttributes.HasFlag(SMBLibrary.FileAttributes.Hidden)) result |= System.IO.FileAttributes.Hidden;
            if (smbFileAttributes.HasFlag(SMBLibrary.FileAttributes.Compressed)) result |= System.IO.FileAttributes.Compressed;
            if (smbFileAttributes.HasFlag(SMBLibrary.FileAttributes.Directory)) result |= System.IO.FileAttributes.Directory;
            if (smbFileAttributes.HasFlag(SMBLibrary.FileAttributes.Encrypted)) result |= System.IO.FileAttributes.Encrypted;
            if (smbFileAttributes.HasFlag(SMBLibrary.FileAttributes.Normal)) result |= System.IO.FileAttributes.Normal;
            if (smbFileAttributes.HasFlag(SMBLibrary.FileAttributes.ReadOnly)) result |= System.IO.FileAttributes.ReadOnly;
            if (smbFileAttributes.HasFlag(SMBLibrary.FileAttributes.IntegrityStream)) result |= System.IO.FileAttributes.IntegrityStream;
            if (smbFileAttributes.HasFlag(SMBLibrary.FileAttributes.NoScrubData)) result |= System.IO.FileAttributes.NoScrubData;
            if (smbFileAttributes.HasFlag(SMBLibrary.FileAttributes.NotContentIndexed)) result |= System.IO.FileAttributes.NotContentIndexed;
            return result;
        }

        public static SMBLibrary.FileAttributes MapFileAttributes(System.IO.FileAttributes fileAttributes)
        {
            SMBLibrary.FileAttributes result = 0;
            if (fileAttributes.HasFlag(System.IO.FileAttributes.System)) result |= SMBLibrary.FileAttributes.System;
            if (fileAttributes.HasFlag(System.IO.FileAttributes.Archive)) result |= SMBLibrary.FileAttributes.Archive;
            if (fileAttributes.HasFlag(System.IO.FileAttributes.Hidden)) result |= SMBLibrary.FileAttributes.Hidden;
            if (fileAttributes.HasFlag(System.IO.FileAttributes.Compressed)) result |= SMBLibrary.FileAttributes.Compressed;
            if (fileAttributes.HasFlag(System.IO.FileAttributes.Directory)) result |= SMBLibrary.FileAttributes.Directory;
            if (fileAttributes.HasFlag(System.IO.FileAttributes.Encrypted)) result |= SMBLibrary.FileAttributes.Encrypted;
            if (fileAttributes.HasFlag(System.IO.FileAttributes.Normal)) result |= SMBLibrary.FileAttributes.Normal;
            if (fileAttributes.HasFlag(System.IO.FileAttributes.ReadOnly)) result |= SMBLibrary.FileAttributes.ReadOnly;
            if (fileAttributes.HasFlag(System.IO.FileAttributes.IntegrityStream)) result |= SMBLibrary.FileAttributes.IntegrityStream;
            if (fileAttributes.HasFlag(System.IO.FileAttributes.NoScrubData)) result |= SMBLibrary.FileAttributes.NoScrubData;
            if (fileAttributes.HasFlag(System.IO.FileAttributes.NotContentIndexed)) result |= SMBLibrary.FileAttributes.NotContentIndexed;
            return result;
        }
    }
}
