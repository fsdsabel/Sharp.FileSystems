using Sharp.FileSystems.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sharp.FileSystem.Smb
{
    internal class SmbFile : IFile
    {
        public SmbFile(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }

        public IFileSystem FileSystem { get; }

        public void AppendAllLines(string path, IEnumerable<string> contents)
        {
            AppendAllLines(path, contents, Encoding.UTF8);
        }

        public void AppendAllLines(string path, IEnumerable<string> contents, Encoding encoding)
        {
            var stream = FileSystem.FileInfo.FromFileName(path).Open(FileMode.Append, FileAccess.Write);
            stream.Position = stream.Length;
            using (var writer = new StreamWriter(stream, encoding))
            {
                foreach (var line in contents)
                {
                    writer.WriteLine(line);
                }
            }
        }

        public void AppendAllText(string path, string contents)
        {
            AppendAllText(path, contents, Encoding.UTF8);
        }

        public void AppendAllText(string path, string contents, Encoding encoding)
        {
            var stream = FileSystem.FileInfo.FromFileName(path).Open(FileMode.Append, FileAccess.Write);
            stream.Position = stream.Length;
            using (var writer = new StreamWriter(stream, encoding))
            {
                writer.Write(contents);
            }
        }

        public StreamWriter AppendText(string path)
        {
            return FileSystem.FileInfo.FromFileName(path).AppendText();
        }

        public void Copy(string sourceFileName, string destFileName)
        {
            Copy(sourceFileName, destFileName, false);
        }

        public void Copy(string sourceFileName, string destFileName, bool overwrite)
        {
            FileSystem.FileInfo.FromFileName(sourceFileName).CopyTo(destFileName, overwrite);
        }

        public Stream Create(string path)
        {
            return Create(path, 4096);
        }

        public Stream Create(string path, int bufferSize)
        {
            return Create(path, bufferSize, FileOptions.None);
        }

        public Stream Create(string path, int bufferSize, FileOptions options)
        {
            return new BufferedStream(FileSystem.FileInfo.FromFileName(path).Create(), bufferSize);
        }

        public StreamWriter CreateText(string path)
        {
            return FileSystem.FileInfo.FromFileName(path).CreateText();
        }

        public void Decrypt(string path)
        {
            throw new NotSupportedException();
        }

        public void Delete(string path)
        {
            FileSystem.FileInfo.FromFileName(path).Delete();
        }

        public void Encrypt(string path)
        {
            throw new NotSupportedException();
        }

        public bool Exists(string path)
        {
            var info = FileSystem.FileInfo.FromFileName(path);
            info.Refresh();
            return info.Exists;
        }

        public FileAttributes GetAttributes(string path)
        {
            var info = FileSystem.FileInfo.FromFileName(path);
            info.Refresh();
            return info.Attributes;
        }

        public DateTime GetCreationTime(string path)
        {
            var info = FileSystem.FileInfo.FromFileName(path);
            info.Refresh();
            return info.CreationTime;
        }

        public DateTime GetCreationTimeUtc(string path)
        {
            var info = FileSystem.FileInfo.FromFileName(path);
            info.Refresh();
            return info.CreationTimeUtc;
        }

        public DateTime GetLastAccessTime(string path)
        {
            var info = FileSystem.FileInfo.FromFileName(path);
            info.Refresh();
            return info.LastAccessTime;
        }

        public DateTime GetLastAccessTimeUtc(string path)
        {
            var info = FileSystem.FileInfo.FromFileName(path);
            info.Refresh();
            return info.LastAccessTimeUtc;
        }

        public DateTime GetLastWriteTime(string path)
        {
            var info = FileSystem.FileInfo.FromFileName(path);
            info.Refresh();
            return info.LastWriteTime;
        }

        public DateTime GetLastWriteTimeUtc(string path)
        {
            var info = FileSystem.FileInfo.FromFileName(path);
            info.Refresh();
            return info.LastWriteTimeUtc;
        }

        public void Move(string sourceFileName, string destFileName)
        {
            FileSystem.FileInfo.FromFileName(sourceFileName).MoveTo(destFileName);
        }

        public Stream Open(string path, FileMode mode)
        {
            return Open(path, mode, FileAccess.ReadWrite);
        }

        public Stream Open(string path, FileMode mode, FileAccess access)
        {
            return Open(path, mode, access, FileShare.None);
        }

        public Stream Open(string path, FileMode mode, FileAccess access, FileShare share)
        {
            return FileSystem.FileInfo.FromFileName(path).Open(mode, access, share);
        }

        public Stream OpenRead(string path)
        {
            return Open(path, FileMode.Open, FileAccess.Read);
        }

        public StreamReader OpenText(string path)
        {
            return new StreamReader(OpenRead(path));
        }

        public Stream OpenWrite(string path)
        {
            return Open(path, FileMode.OpenOrCreate, FileAccess.Write);
        }

        public byte[] ReadAllBytes(string path)
        {
            using (var file = Open(path, FileMode.Open, FileAccess.Read))
            {
                var result = new byte[file.Length];
                var toread = result.Length;
                int read = 0;
                while (toread > 0)
                {
                    var cread = file.Read(result, read, 4096);
                    toread -= cread;
                    read += cread;
                }
                return result;
            }
        }

        public string[] ReadAllLines(string path)
        {
            return ReadAllLines(path, Encoding.UTF8);
        }

        public string[] ReadAllLines(string path, Encoding encoding)
        {
            return ReadLines(path, encoding).ToArray();
        }

        public string ReadAllText(string path)
        {
            return ReadAllText(path, Encoding.UTF8);
        }

        public string ReadAllText(string path, Encoding encoding)
        {
            using (var reader = new StreamReader(FileSystem.FileInfo.FromFileName(path).Open(FileMode.Open, FileAccess.Read), encoding))
            {
                return reader.ReadToEnd();
            }
        }

        public IEnumerable<string> ReadLines(string path)
        {
            return ReadLines(path, Encoding.UTF8);
        }

        public IEnumerable<string> ReadLines(string path, Encoding encoding)
        {
            using (var reader = new StreamReader(FileSystem.FileInfo.FromFileName(path).Open(FileMode.Open, FileAccess.Read), encoding))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        public void SetAttributes(string path, FileAttributes fileAttributes)
        {
            FileSystem.FileInfo.FromFileName(path).Attributes = fileAttributes;
        }

        public void SetCreationTime(string path, DateTime creationTime)
        {
            FileSystem.FileInfo.FromFileName(path).CreationTime = creationTime;
        }

        public void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
        {
            FileSystem.FileInfo.FromFileName(path).CreationTimeUtc = creationTimeUtc;
        }

        public void SetLastAccessTime(string path, DateTime lastAccessTime)
        {
            FileSystem.FileInfo.FromFileName(path).LastAccessTime = lastAccessTime;
        }

        public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
        {
            FileSystem.FileInfo.FromFileName(path).LastAccessTimeUtc = lastAccessTimeUtc;
        }

        public void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            FileSystem.FileInfo.FromFileName(path).LastWriteTime = lastWriteTime;
        }

        public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
        {
            FileSystem.FileInfo.FromFileName(path).LastWriteTimeUtc = lastWriteTimeUtc;
        }

        public void WriteAllBytes(string path, byte[] bytes)
        {
            using (var stream = OpenWrite(path))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        public void WriteAllLines(string path, IEnumerable<string> contents)
        {
            WriteAllLines(path, contents, Encoding.UTF8);
        }

        public void WriteAllLines(string path, IEnumerable<string> contents, Encoding encoding)
        {
            using (var writer = new StreamWriter(OpenWrite(path), encoding))
            {
                foreach (var line in contents)
                {
                    writer.WriteLine(line);
                }
            }
        }

        public void WriteAllLines(string path, string[] contents)
        {
            WriteAllLines(path, contents, Encoding.UTF8);
        }

        public void WriteAllLines(string path, string[] contents, Encoding encoding)
        {
            WriteAllLines(path, (IEnumerable<string>)contents, encoding);
        }

        public void WriteAllText(string path, string contents)
        {
            WriteAllText(path, contents, Encoding.UTF8);
        }

        public void WriteAllText(string path, string contents, Encoding encoding)
        {
            using (var writer = new StreamWriter(OpenWrite(path), encoding))
            {
                writer.Write(contents);
            }
        }
    }
}
