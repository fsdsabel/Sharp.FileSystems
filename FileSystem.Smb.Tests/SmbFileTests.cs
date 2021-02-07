using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace FileSystem.Smb.Tests
{
    [TestClass]
    public class SmbFileTests
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
        public void Can_write_to_and_read_from_file(ShareType shareType)
        {
            var fs = new SmbFileSystem();
            CreateTestFolder(shareType);

            var data = new byte[1024 * 1024 * 4];
            var rand = new Random(23238);
            for (int i = 0; i < data.Length; i++) data[i] = (byte)(i % 255);//(byte)rand.Next(256);

            using (var file = fs.File.Create(TestHelpers.ShareUri($"{TestFolder}/data.bin", shareType)))
            {
                file.Write(data, 0, data.Length);
            }

            using (var file = fs.File.OpenRead(TestHelpers.ShareUri($"{TestFolder}/data.bin", shareType)))
            {
                var readBuffer = new byte[data.Length];
                file.Length.Should().Be(data.Length);
                file.Read(readBuffer, 0, readBuffer.Length).Should().Be(data.Length);
                CollectionAssert.AreEqual(data, readBuffer);
            }
        }

        [DataTestMethod]
        [TestShare]
        public void Can_create_and_delete_a_file(ShareType shareType)
        {
            var fs = new SmbFileSystem();
            CreateTestFolder(shareType);

            var filename = TestHelpers.ShareUri($"{TestFolder}/data.bin", shareType);

            fs.FileInfo.FromFileName(filename).Exists.Should().BeFalse();

            fs.File.Create(filename).Close();

            fs.FileInfo.FromFileName(filename).Exists.Should().BeTrue();

            fs.File.Delete(filename);

            fs.FileInfo.FromFileName(filename).Exists.Should().BeFalse();
        }

        [DataTestMethod]
        [TestShare]
        public void Can_write_and_read_all_text_to_file(ShareType shareType)
        {
            const string testText = "Test text äöü";
            var fs = new SmbFileSystem();
            CreateTestFolder(shareType);
            var filename = TestHelpers.ShareUri($"{TestFolder}/test.txt", shareType);
            fs.File.WriteAllText(filename, testText, Encoding.UTF32);

            fs.File.ReadAllText(filename, Encoding.UTF32).Should().Be(testText);
        }

        [DataTestMethod]
        [TestShare]
        public void Can_write_and_read_all_lines_to_file(ShareType shareType)
        {
            string[] testText = new[] { "Test text äöü", "line 2 äsad", "", "line 3" };
            var fs = new SmbFileSystem();
            CreateTestFolder(shareType);
            var filename = TestHelpers.ShareUri($"{TestFolder}/testlines.txt", shareType);
            fs.File.WriteAllLines(filename, testText, Encoding.UTF32);

            fs.File.ReadAllLines(filename, Encoding.UTF32).Should().BeEquivalentTo(testText);
        }

        [DataTestMethod]
        [TestShare]
        public void Can_write_and_read_all_bytes_to_file(ShareType shareType)
        {
            var rand = new Random(12345);
            var data = new byte[1024 * 1024 * 4];
            rand.NextBytes(data);
            var fs = new SmbFileSystem();
            CreateTestFolder(shareType);

            var filename = TestHelpers.ShareUri($"{TestFolder}/testbytes.bin", shareType);
            fs.File.WriteAllBytes(filename, data);

            var read = fs.File.ReadAllBytes(filename);

            CollectionAssert.AreEqual(data, read);
        }

        [DataTestMethod]
        [TestShare]
        public void Can_get_and_set_file_attributes(ShareType shareType)
        {
            var fs = new SmbFileSystem();
            CreateTestFolder(shareType);
            var filename = TestHelpers.ShareUri($"{TestFolder}/testattributes.txt", shareType);

            fs.File.Create(filename).Close();

            fs.File.SetAttributes(filename, FileAttributes.Hidden | FileAttributes.System | FileAttributes.Archive);

            var attributes = fs.File.GetAttributes(filename);
            attributes.Should().HaveFlag(FileAttributes.Hidden);
            attributes.Should().HaveFlag(FileAttributes.System);
            attributes.Should().HaveFlag(FileAttributes.Archive);
        }

        [DataTestMethod]
        [TestShare]
        public void Can_get_and_set_file_times(ShareType shareType)
        {
            var fs = new SmbFileSystem();
            CreateTestFolder(shareType);
            var filename = TestHelpers.ShareUri($"{TestFolder}/testattributes.txt", shareType);

            fs.File.Create(filename).Close();

            fs.File.SetAttributes(filename, FileAttributes.System);

            fs.File.SetCreationTime(filename, new DateTime(1980, 2, 20, 1, 0, 0));
            fs.File.GetCreationTime(filename).Should().Be(new DateTime(1980, 2, 20, 1, 0, 0));

            fs.File.SetCreationTimeUtc(filename, new DateTime(1980, 2, 20, 12, 0, 0));
            fs.File.GetCreationTimeUtc(filename).Should().Be(new DateTime(1980, 2, 20, 12, 0, 0));

            fs.File.SetLastAccessTime(filename, new DateTime(1980, 2, 20, 3, 0, 0));
            fs.File.GetLastAccessTime(filename).Should().Be(new DateTime(1980, 2, 20, 3, 0, 0));

            fs.File.SetLastAccessTimeUtc(filename, new DateTime(1980, 2, 20, 14, 0, 0));
            fs.File.GetLastAccessTimeUtc(filename).Should().Be(new DateTime(1980, 2, 20, 14, 0, 0));

            fs.File.SetLastWriteTime(filename, new DateTime(1980, 2, 20, 5, 0, 0));
            fs.File.GetLastWriteTime(filename).Should().Be(new DateTime(1980, 2, 20, 5, 0, 0));

            fs.File.SetLastWriteTimeUtc(filename, new DateTime(1980, 2, 20, 16, 0, 0));
            fs.File.GetLastWriteTimeUtc(filename).Should().Be(new DateTime(1980, 2, 20, 16, 0, 0));

            fs.File.GetAttributes(filename).Should().HaveFlag(FileAttributes.System);
        }


        [DataTestMethod]
        [TestShare]
        public void Can_check_file_exists(ShareType shareType)
        {
            var fs = new SmbFileSystem();
            CreateTestFolder(shareType);
            var filename = TestHelpers.ShareUri($"{TestFolder}/somefile.txt", shareType);

            fs.File.Exists(filename).Should().BeFalse();

            fs.File.Create(filename).Close();

            fs.File.Exists(filename).Should().BeTrue();
        }

        [DataTestMethod]
        [TestShare]
        public void Can_append_and_read_all_text_to_file(ShareType shareType)
        {
            const string testText = "Test text äöü";
            var fs = new SmbFileSystem();
            CreateTestFolder(shareType);
            var filename = TestHelpers.ShareUri($"{TestFolder}/test.txt", shareType);
            fs.File.WriteAllText(filename, testText, Encoding.UTF32);
            fs.File.AppendAllText(filename, testText, Encoding.UTF32);

            fs.File.ReadAllText(filename, Encoding.UTF32).Should().Be(testText + testText);
        }

        [DataTestMethod]
        [TestShare]
        public void Can_append_and_read_all_lines_to_file(ShareType shareType)
        {
            string[] testText = new[] { "Test text äöü", "line 2 äsad", "", "line 3" };
            var fs = new SmbFileSystem();
            CreateTestFolder(shareType);
            var filename = TestHelpers.ShareUri($"{TestFolder}/testlines.txt", shareType);
            fs.File.WriteAllLines(filename, testText, Encoding.UTF32);
            fs.File.AppendAllLines(filename, testText, Encoding.UTF32);

            fs.File.ReadAllLines(filename, Encoding.UTF32).Should().BeEquivalentTo(testText.Concat(testText));
        }


        [DataTestMethod]
        [TestShare]
        public void Can_copy_file_with_overwrite(ShareType shareType)
        {            
            var fs = new SmbFileSystem();
            CreateTestFolder(shareType);
            var srcFilename = TestHelpers.ShareUri($"{TestFolder}/source.txt", shareType);
            var dstFilename = TestHelpers.ShareUri($"{TestFolder}/dest.txt", shareType);

            fs.File.WriteAllText(srcFilename, "test");
            fs.File.WriteAllText(dstFilename, "dest");

            fs.File.Copy(srcFilename, dstFilename, true);

            fs.File.ReadAllText(srcFilename).Should().Be("test");
            fs.File.ReadAllText(dstFilename).Should().Be("test");
        }

        [DataTestMethod]
        [TestShare]
        [ExpectedException(typeof(System.IO.IOException))]
        public void Cannot_copy_file_without_overwrite_when_existing(ShareType shareType)
        {
            var fs = new SmbFileSystem();
            CreateTestFolder(shareType);
            var srcFilename = TestHelpers.ShareUri($"{TestFolder}/source.txt", shareType);
            var dstFilename = TestHelpers.ShareUri($"{TestFolder}/dest.txt", shareType);

            fs.File.WriteAllText(srcFilename, "test");
            fs.File.WriteAllText(dstFilename, "dest");

            fs.File.Copy(srcFilename, dstFilename, false);
        }

        [DataTestMethod]
        [TestShare]
        public void Can_move_file(ShareType shareType)
        {
            var fs = new SmbFileSystem();
            CreateTestFolder(shareType);
            var srcFilename = TestHelpers.ShareUri($"{TestFolder}/source.txt", shareType);
            var dstFilename = TestHelpers.ShareUri($"{TestFolder}/dest.txt", shareType);

            fs.File.WriteAllText(srcFilename, "test");
            fs.File.WriteAllText(dstFilename, "dest");

            fs.File.Move(srcFilename, dstFilename);

            fs.File.Exists(srcFilename).Should().BeFalse("file has been moved");
            fs.File.ReadAllText(dstFilename).Should().Be("test");
        }
    }
}
