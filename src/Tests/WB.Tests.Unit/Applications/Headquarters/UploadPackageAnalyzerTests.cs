using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.Infrastructure.FileSystem;
using WB.UI.Headquarters.Services.Maps;

namespace WB.Tests.Unit.Applications.Headquarters;

[TestOf(typeof(UploadPackageAnalyzer))]
public class UploadPackageAnalyzerTests
{
    private class FakeFileSystemAccessor : IFileSystemAccessor
    {
        public string CombinePath(string path1, string path2) => Path.Combine(path1, path2);
        public string CombinePath(params string[] paths) => Path.Combine(paths);
        public string GetFileName(string filePath) => Path.GetFileName(filePath);
        public string GetFileNameWithoutExtension(string filePath) => Path.GetFileNameWithoutExtension(filePath);
        public string GetFileExtension(string filePath) => Path.GetExtension(filePath)?.ToLowerInvariant();
        public long GetFileSize(string filePath) => -1;
        public long GetDirectorySize(string path) => -1;
        public System.DateTime GetCreationTime(string filePath) => System.DateTime.MinValue;
        public System.DateTime GetModificationTime(string filePath) => System.DateTime.MinValue;
        public bool IsDirectoryExists(string pathToDirectory) => false;
        public void CreateDirectory(string path) {}
        public void DeleteDirectory(string path) {}
        public string GetDirectory(string path) => Path.GetDirectoryName(path);
        public string GetDirectoryName(string path) => new DirectoryInfo(path).Name;
        public bool IsFileExists(string pathToFile) => false;
        public Stream OpenOrCreateFile(string pathToFile, bool append) => new MemoryStream();
        public Stream ReadFile(string pathToFile) => new MemoryStream();
        public void DeleteFile(string pathToFile) {}
        public string MakeStataCompatibleFileName(string name) => name;
        public string MakeValidFileName(string name) => name;
        public string[] GetDirectoriesInDirectory(string pathToDirectory) => new string[0];
        public string[] GetFilesInDirectory(string pathToDirectory, bool searchInSubdirectories = false) => new string[0];
        public string[] GetFilesInDirectory(string pathToDirectory, string pattern, bool searchInSubdirectories = false) => new string[0];
        public void WriteAllText(string pathToFile, string content) {}
        public System.Threading.Tasks.Task WriteAllTextAsync(string pathToFile, string content) => System.Threading.Tasks.Task.CompletedTask;
        public void WriteAllBytes(string pathToFile, byte[] content) {}
        public System.Threading.Tasks.Task WriteAllBytesAsync(string pathToFile, byte[] content, System.Threading.CancellationToken token = default) => System.Threading.Tasks.Task.CompletedTask;
        public byte[] ReadHash(string pathToFile) => new byte[0];
        public byte[] ReadHash(Stream fileContent) => new byte[0];
        public bool IsHashValid(byte[] fileContent, byte[] hash) => true;
        public byte[] ReadAllBytes(string pathToFile, long? start = null, long? length = null) => new byte[0];
        public string ReadAllText(string pathToFile) => string.Empty;
        public void CopyFileOrDirectory(string sourceDir, string targetDir, bool overrideAll = false, string[] fileExtensionsFilter = null) {}
        public void MarkFileAsReadonly(string pathToFile) {}
        public string ChangeExtension(string path1, string newExtension) => Path.ChangeExtension(path1, newExtension);
        public void MoveFile(string pathToFile, string newPathToFile) {}
        public void MoveDirectory(string pathToDir, string newPathToDir) {}
        public char[] GetInvalidFileNameChars() => Path.GetInvalidFileNameChars();
        public bool IsInvalidFileName(string filename) => false;
    }

    private class FakeArchiveUtils : IArchiveUtils
    {
        private readonly Dictionary<string,long> files;
        public FakeArchiveUtils(Dictionary<string,long> files) { this.files = files; }
        public void ExtractToDirectory(string archivedFile, string extractToFolder, bool ignoreRootDirectory = false) {}
        public void ExtractToDirectory(Stream archivedFile, string extractToFolder, bool ignoreRootDirectory = false) {}
        public ExtractedFile GetFileFromArchive(string archiveFilePath, string fileName) => null;
        public ExtractedFile GetFileFromArchive(byte[] archivedFileAsArray, string fileName) => null;
        public IList<ExtractedFile> GetFilesFromArchive(Stream inputStream) => new List<ExtractedFile>();
        public bool IsZipStream(Stream zipStream) => true;
        public Dictionary<string, long> GetFileNamesAndSizesFromArchive(byte[] archivedFileAsArray) => files;
        public Dictionary<string, long> GetFileNamesAndSizesFromArchive(Stream inputStream) => files;
        public string CompressString(string stringToCompress) => stringToCompress;
        public string DecompressString(string stringToDecompress) => stringToDecompress;
        public byte[] CompressContentToSingleFile(byte[] uncompressedData, string entryName) => uncompressedData;
        public void CreateArchiveFromDirectory(string directory, string archiveFile) {}
        public void CreateArchiveFromFileList(IEnumerable<string> files, string archiveFilePath) {}
    }

    [Test]
    public void Analyze_should_set_Name_to_filename_with_extension_for_permitted_map_files()
    {
        var files = new Dictionary<string, long>
        {
            {"folder/sub/map1.tpk", 100},
            {"another/path/map2.mmpk", 200},
            {"root/map3.tif", 300},
        };
        var analyzer = new UploadPackageAnalyzer(new FakeFileSystemAccessor(), new FakeArchiveUtils(files));

        using var dummyStream = new MemoryStream(new byte[] {1,2,3});
        var result = analyzer.Analyze(dummyStream);

        var names = result.Maps.Select(m => m.Name).ToList();
        Assert.That(names, Does.Contain("map1.tpk"));
        Assert.That(names, Does.Contain("map2.mmpk"));
        Assert.That(names, Does.Contain("map3.tif"));
        // ensure no paths leaked
        Assert.That(names.Any(n => n.Contains("/")), Is.False);
    }

    [Test]
    public void Analyze_should_group_shapefile_and_set_Name_to_filename_without_extension()
    {
        var files = new Dictionary<string, long>
        {
            {"dir/region.shp", 10},
            {"dir/region.dbf", 20},
            {"dir/region.shx", 30},
            {"dir/region.prj", 5},
        };
        var analyzer = new UploadPackageAnalyzer(new FakeFileSystemAccessor(), new FakeArchiveUtils(files));

        using var dummyStream = new MemoryStream(new byte[] {1,2,3});
        var result = analyzer.Analyze(dummyStream);

        var shapefile = result.Maps.Single(m => m.IsShapeFile);
        Assert.That(shapefile.Name, Is.EqualTo("region"));
        Assert.That(shapefile.Files.Select(f => f.Name).OrderBy(x => x), Is.EquivalentTo(files.Keys.OrderBy(x => x)));
        // ensure group is not named with extension
        Assert.That(shapefile.Name.EndsWith(".shp") || shapefile.Name.EndsWith(".dbf") || shapefile.Name.EndsWith(".shx"), Is.False);
    }
}

