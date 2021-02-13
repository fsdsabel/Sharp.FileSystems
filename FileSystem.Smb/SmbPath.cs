using Sharp.FileSystems.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sharp.FileSystem.Smb
{
    class SmbPath : IPath
    {

        public SmbPath(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }

        public IFileSystem FileSystem { get; }

        public string Combine(params string[] paths)
        {
            //new Uri(paths[0], UriKind.RelativeOrAbsolute)
            // TODO we need to be more sophiticated here 
            return string.Join("/", paths);
        }
    }
}
