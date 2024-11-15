using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using NUnit.Framework;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;

namespace WB.Tests.Integration.UtilsTest
{
    [TestOf(typeof(ZipArchiveUtils))]
    [TestFixture]
    internal class ZipArchiveUtilsTests
    {
        private ZipArchiveUtils zipArchiveUtils;

        [OneTimeSetUp]
        public void SetUp()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            zipArchiveUtils = new ZipArchiveUtils();
        }

        [Test]
        public void should_preserve_zero_length_files_in_archive()
        {
            var filePath = $"test_{Guid.NewGuid()}.zip";

            try
            {
                using (var fs = File.Create(filePath))
                {
                    using (var arch = new CompressionZipArchive(fs))
                    {
                        arch.CreateEntry("non_empty.file", Encoding.UTF8.GetBytes("test"));
                        arch.CreateEntry("empty.file", Array.Empty<byte>());
                    }
                }

                using (var zip = ZipFile.OpenRead(filePath))
                {
                    var nonEmpty = zip.Entries.SingleOrDefault(e => e.Name == "non_empty.file");
                    Assert.That(nonEmpty, Is.Not.Null);

                    var emptyFile = zip.Entries.SingleOrDefault(e => e.Name == "empty.file");
                    Assert.That(emptyFile, Is.Not.Null);

                    using var streamEmpty = emptyFile.Open();
                    using (var ms = new MemoryStream())
                    {
                        streamEmpty.CopyTo(ms);
                        Assert.That(ms.ToArray(), Has.Length.EqualTo(0));
                    }

                    using var stream = nonEmpty.Open();
                    using (var ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        Assert.That(ms.ToArray(), Has.Length.EqualTo(4));
                    }
                }
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Test]
        public void should_compress_and_decompress_string()
        {
            const string originalString = "This is a test string to compress and decompress.";
            var compressedString = zipArchiveUtils.CompressString(originalString);
            var decompressedString = zipArchiveUtils.DecompressString(compressedString);

            Assert.That(decompressedString, Is.EqualTo(originalString));
        }

        [Test]
        public void should_create_archive_from_directory()
        {
            var testDirectory = $"test_directory_{Guid.NewGuid()}";
            var testFile = Path.Combine(testDirectory, "test_file.txt");
            var archiveFile = $"test_archive_{Guid.NewGuid()}.zip";

            Directory.CreateDirectory(testDirectory);
            File.WriteAllText(testFile, "Test content");

            try
            {
                zipArchiveUtils.CreateArchiveFromDirectory(testDirectory, archiveFile);

                Assert.That(File.Exists(archiveFile), Is.True);
            }
            finally
            {
                Directory.Delete(testDirectory, true);
                File.Delete(archiveFile);
            }
        }

        [Test]
        public void should_create_archive_from_file_list()
        {
            var testFiles = new List<string>
            {
                $"file1_{Guid.NewGuid()}.txt",
                $"file2_{Guid.NewGuid()}.txt"
            };
            var archiveFile = $"test_archive_{Guid.NewGuid()}.zip";

            foreach (var file in testFiles)
            {
                File.WriteAllText(file, "Test content");
            }

            try
            {
                zipArchiveUtils.CreateArchiveFromFileList(testFiles, archiveFile);

                Assert.That(File.Exists(archiveFile), Is.True);
            }
            finally
            {
                foreach (var file in testFiles)
                {
                    File.Delete(file);
                }
                File.Delete(archiveFile);
            }
        }

        [Test]
        public void should_extract_to_directory()
        {
            var testDirectory = $"test_directory_{Guid.NewGuid()}";
            var testFile = Path.Combine(testDirectory, "test_file.txt");
            var archiveFile = $"test_archive_{Guid.NewGuid()}.zip";
            var extractDirectory = $"extracted_directory_{Guid.NewGuid()}";

            Directory.CreateDirectory(testDirectory);
            File.WriteAllText(testFile, "Test content");

            try
            {
                zipArchiveUtils.CreateArchiveFromDirectory(testDirectory, archiveFile);
                zipArchiveUtils.ExtractToDirectory(archiveFile, extractDirectory, true);

                Assert.That(Directory.Exists(extractDirectory), Is.True);
                Assert.That(File.Exists(Path.Combine(extractDirectory, "test_file.txt")), Is.True);
            }
            finally
            {
                Directory.Delete(testDirectory, true);
                Directory.Delete(extractDirectory, true);
                File.Delete(archiveFile);
            }
        }

        [Test]
        public void should_get_file_names_and_sizes_from_archive()
        {
            var testFiles = new List<string>
            {
                $"file1_{Guid.NewGuid()}.txt",
                $"file2_{Guid.NewGuid()}.txt"
            };
            var archiveFile = $"test_archive_{Guid.NewGuid()}.zip";

            foreach (var file in testFiles)
            {
                File.WriteAllText(file, "Test content");
            }

            try
            {
                zipArchiveUtils.CreateArchiveFromFileList(testFiles, archiveFile);
                
                var fileNamesAndSizes = zipArchiveUtils.GetFileNamesAndSizesFromArchive(File.ReadAllBytes(archiveFile));

                Assert.That(fileNamesAndSizes.Count, Is.EqualTo(testFiles.Count));
                foreach (var file in testFiles)
                {
                    Assert.That(fileNamesAndSizes.ContainsKey(Path.GetFileName(file)), Is.True);
                }
            }
            finally
            {
                foreach (var file in testFiles)
                {
                    File.Delete(file);
                }
                File.Delete(archiveFile);
            }
        }
        
        [Test]
        public void should_return_true_for_valid_zip_stream()
        {
            var zipArchiveUtils = new ZipArchiveUtils();
            var testFile = $"test_{Guid.NewGuid()}.zip";

            try
            {
                using (var fileStream = new FileStream(testFile, FileMode.Create))
                using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
                {
                    var entry = archive.CreateEntry("test_file.txt");
                    using (var entryStream = entry.Open())
                    using (var writer = new StreamWriter(entryStream))
                    {
                        writer.Write("Test content");
                    }
                }

                using (var fileStream = new FileStream(testFile, FileMode.Open, FileAccess.Read))
                {
                    var result = zipArchiveUtils.IsZipStream(fileStream);
                    Assert.That(result, Is.True);
                }
            }
            finally
            {
                File.Delete(testFile);
            }
        }

        [Test]
        public void should_return_false_for_invalid_zip_stream()
        {
            var zipArchiveUtils = new ZipArchiveUtils();
            var testFile = $"test_{Guid.NewGuid()}.txt";

            try
            {
                File.WriteAllText(testFile, "This is not a zip file.");

                using (var fileStream = new FileStream(testFile, FileMode.Open, FileAccess.Read))
                {
                    var result = zipArchiveUtils.IsZipStream(fileStream);
                    Assert.That(result, Is.False);
                }
            }
            finally
            {
                File.Delete(testFile);
            }
        }
    }
}
