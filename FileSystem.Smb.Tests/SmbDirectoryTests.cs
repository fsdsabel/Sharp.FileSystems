using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sharp.FileSystem.Smb.Tests
{
    [TestClass]
    public class SmbDirectoryTests
    {
        private static readonly string TestFolder = Guid.NewGuid().ToString();


        [TestCleanup]
        public void Cleanup()
        {
            new SmbFileSystem().Directory.Delete(TestHelpers.ShareUri(TestFolder, ShareType.Anonymous), true);
        }
        private void CreateTestFolder(ShareType shareType)
        {
            new SmbFileSystem().Directory.CreateDirectory(TestHelpers.ShareUri(TestFolder, shareType));
        }

        [DataTestMethod]
        [TestShare]
        public void Can_Create_Smb_Directory(ShareType shareType)
        {
            var info = new SmbFileSystem().Directory.CreateDirectory(TestHelpers.ShareUri($"{TestFolder}/Cifs.Fs", shareType));
            info.Exists.Should().BeTrue();
            info.FullName.Should().Be($"{TestFolder}/Cifs.Fs");
            info.Name.Should().Be("Cifs.Fs");
            info.Extension.Should().Be(".Fs");
            info.LastAccessTime.Should().BeCloseTo(DateTime.Now, 1000);
            info.LastAccessTimeUtc.Should().BeCloseTo(DateTime.UtcNow, 1000);
            info.LastWriteTime.Should().BeCloseTo(DateTime.Now, 1000);
            info.LastWriteTimeUtc.Should().BeCloseTo(DateTime.UtcNow, 1000);
            info.CreationTime.Should().BeCloseTo(DateTime.Now, 1000);
            info.CreationTimeUtc.Should().BeCloseTo(DateTime.UtcNow, 1000);
            info.Attributes.Should().Be(FileAttributes.Directory);
        }

        [DataTestMethod]
        [TestShare]
        public void Can_enumerate_directories_in_smb_directory(ShareType shareType)
        {
            var dirList = new List<string>
            {
                $"{TestFolder}/Cifs.Fs",
                $"{TestFolder}/Cifs.Fs.2"
            };

            var fs = new SmbFileSystem();
            foreach (var dir in dirList)
            {
                fs.Directory.CreateDirectory(TestHelpers.ShareUri(dir, shareType));
            }

            var found = fs.Directory.EnumerateDirectories(TestHelpers.ShareUri(TestFolder, shareType));
            found.Should().BeEquivalentTo(dirList);
        }

        [DataTestMethod]
        [TestShare]
        public void Can_enumerate_directories_in_smb_directory_with_pattern(ShareType shareType)
        {
            var dirList = new List<string>
            {
                $"{TestFolder}/Cifs.Fs",
                $"{TestFolder}/Cifs.Fs.2",
                $"{TestFolder}/Cifs.Fs.2/test.2"
            };

            var fs = new SmbFileSystem();
            foreach (var dir in dirList)
            {
                fs.Directory.CreateDirectory(TestHelpers.ShareUri(dir, shareType));
            }

            var found = fs.Directory.EnumerateDirectories(TestHelpers.ShareUri(TestFolder, shareType), "*.2").ToArray();
            found.Should().OnlyContain(f => f == dirList[1]);
        }

        [DataTestMethod]
        [TestShare]
        public void Can_enumerate_directories_in_smb_directory_with_pattern_and_deep(ShareType shareType)
        {
            var dirList = new List<string>
            {
                $"{TestFolder}/Cifs.Fs",
                $"{TestFolder}/Cifs.Fs.2",
                $"{TestFolder}/Cifs.Fs.2/test.2"
            };

            var fs = new SmbFileSystem();
            foreach (var dir in dirList)
            {
                fs.Directory.CreateDirectory(TestHelpers.ShareUri(dir, shareType));
            }

            var found = fs.Directory.EnumerateDirectories(TestHelpers.ShareUri(TestFolder, shareType), "*.2", SearchOption.AllDirectories).ToArray();
            found.Should().HaveCount(2);
            found.Should().Contain(dirList[1]);
            found.Should().Contain(dirList[2]);
        }

        [DataTestMethod]
        [TestShare]
        public void Can_enumerate_files_in_smb_directory_with_pattern_and_deep(ShareType shareType)
        {
            var dirList = new List<string>
            {
                $"{TestFolder}/Cifs.Fs",
                $"{TestFolder}/Cifs.Fs.2",
                $"{TestFolder}/Cifs.Fs.2/test.2"
            };

            var fileList = new List<string>
            {
                $"{TestFolder}/blubb.txt",
                $"{TestFolder}/Cifs.Fs/test.jpg",
                $"{TestFolder}/Cifs.Fs.2/test.2/done.txt"
            };

            var fs = new SmbFileSystem();
            foreach (var dir in dirList)
            {
                fs.Directory.CreateDirectory(TestHelpers.ShareUri(dir, shareType));
            }
            foreach (var file in fileList)
            {
                fs.File.Create(TestHelpers.ShareUri(file, shareType)).Close();
            }

            var found = fs.Directory.EnumerateFiles(TestHelpers.ShareUri(TestFolder, shareType), "*.txt", SearchOption.AllDirectories).ToArray();
            found.Should().HaveCount(2);
            found.Should().Contain(fileList[0]);
            found.Should().Contain(fileList[2]);
        }

        [DataTestMethod]
        [TestShare]
        public void Can_get_and_set_directory_times(ShareType shareType)
        {
            var fs = new SmbFileSystem();
            CreateTestFolder(shareType);
            var dirname = TestHelpers.ShareUri($"{TestFolder}/testattributes", shareType);

            fs.Directory.CreateDirectory(dirname);



            fs.Directory.SetCreationTime(dirname, new DateTime(1980, 2, 20, 1, 0, 0));
            fs.Directory.GetCreationTime(dirname).Should().Be(new DateTime(1980, 2, 20, 1, 0, 0));

            fs.Directory.SetCreationTimeUtc(dirname, new DateTime(1980, 2, 20, 12, 0, 0));
            fs.Directory.GetCreationTimeUtc(dirname).Should().Be(new DateTime(1980, 2, 20, 12, 0, 0));

            fs.Directory.SetLastAccessTime(dirname, new DateTime(1980, 2, 20, 3, 0, 0));
            fs.Directory.GetLastAccessTime(dirname).Should().Be(new DateTime(1980, 2, 20, 3, 0, 0));

            fs.Directory.SetLastAccessTimeUtc(dirname, new DateTime(1980, 2, 20, 14, 0, 0));
            fs.Directory.GetLastAccessTimeUtc(dirname).Should().Be(new DateTime(1980, 2, 20, 14, 0, 0));

            fs.Directory.SetLastWriteTime(dirname, new DateTime(1980, 2, 20, 5, 0, 0));
            fs.Directory.GetLastWriteTime(dirname).Should().Be(new DateTime(1980, 2, 20, 5, 0, 0));

            fs.Directory.SetLastWriteTimeUtc(dirname, new DateTime(1980, 2, 20, 16, 0, 0));
            fs.Directory.GetLastWriteTimeUtc(dirname).Should().Be(new DateTime(1980, 2, 20, 16, 0, 0));
        }

        [DataTestMethod]
        [TestShare]
        public void Can_create_subdirectory(ShareType shareType)
        {
            var fs = new SmbFileSystem();
            CreateTestFolder(shareType);
            var dirname = TestHelpers.ShareUri(TestFolder, ShareType.Anonymous);
            var dir = fs.DirectoryInfo.FromDirectoryName(dirname);

            var info = dir.CreateSubdirectory("sub1/sub2");
            info.Exists.Should().BeTrue();

            fs.DirectoryInfo.FromDirectoryName(TestHelpers.ShareUri($"{TestFolder}/sub1/sub2", ShareType.Anonymous)).Exists.Should().BeTrue();
        }

        [DataTestMethod]
        [TestShare]
        public void Can_move_directory(ShareType shareType)
        {
            var fs = new SmbFileSystem();
            CreateTestFolder(shareType);
            var dirname = TestHelpers.ShareUri($"{TestFolder}/src", ShareType.Anonymous);
            var destname = TestHelpers.ShareUri($"{TestFolder}/dest", ShareType.Anonymous);

            fs.DirectoryInfo.FromDirectoryName(dirname).Create();
            fs.FileInfo.FromFileName(dirname + "/test.txt").Create().Close();

            fs.Directory.Move(dirname, destname);

            fs.DirectoryInfo.FromDirectoryName(dirname).Exists.Should().BeFalse();
            fs.DirectoryInfo.FromDirectoryName(destname).Exists.Should().BeTrue();
            fs.FileInfo.FromFileName(destname + "/test.txt").Exists.Should().BeTrue();
        }

        [TestMethod]
        public void Can_get_directory_root()
        {
            var dirname = TestHelpers.ShareUri($"{TestFolder}/somedir/usdfhu", ShareType.Anonymous);
            new SmbFileSystem().Directory.GetDirectoryRoot(dirname).Should().Be($"/");
        }

        [TestMethod]
        public void Can_get_parent()
        {
            var dirname = TestHelpers.ShareUri($"{TestFolder}/testparent/testchild", ShareType.Anonymous);
            var fs = new SmbFileSystem();
            var parent = fs.DirectoryInfo.FromDirectoryName(dirname).Parent;
            parent.Should().NotBeNull();
            parent.FullName.Should().Be($"{TestFolder}/testparent");

            parent = fs.DirectoryInfo.FromDirectoryName(TestHelpers.ShareUri(TestFolder, ShareType.Anonymous)).Parent;
            parent.Should().NotBeNull();
            parent.FullName.Should().Be("/");

            parent = fs.DirectoryInfo.FromDirectoryName(TestHelpers.ShareUri("/", ShareType.Anonymous)).Parent;
            parent.Should().BeNull("no more parent folders");
        }


        [TestMethod]
        public void Can_get_available_shares()
        {
            var shares = new SmbFileSystem().Directory.GetLogicalDrives($"smb://{TestHelpers.TestHost}");
            shares.Should().Contain(TestHelpers.AnonymousShare);
            shares.Should().Contain(TestHelpers.AuthenticatedShare);
        }
    }
}
