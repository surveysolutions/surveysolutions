using System.Collections.Generic;
using System.IO;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Repositories
{
    internal class FilebasedPreloadedDataRepository : IPreloadedDataRepository
    {
        private readonly IArchiveUtils archiveUtils;
        private readonly ICsvReader csvReader;
        private readonly string[] permittedFileExtensions = { TabExportFile.Extention, TextExportFile.Extension };
        private readonly string importedFile;

        public FilebasedPreloadedDataRepository(string folderPath, IArchiveUtils archiveUtils, ICsvReader csvReader)
        {
            this.archiveUtils = archiveUtils;
            this.csvReader = csvReader;

            var importDirectory = Path.Combine(folderPath, "PreLoadedData");
            if (!Directory.Exists(importDirectory))
                Directory.CreateDirectory(importDirectory);

            this.importedFile = Path.Combine(importDirectory, "assignments");
        }

        public void Store(Stream stream)
        {
            this.DeletePreloadedData();

            stream.Seek(0, SeekOrigin.Begin);

            using (var fileStream = File.Create(this.importedFile))
                stream.CopyTo(fileStream);
        }

        public PreloadedDataByFile GetPreloadedDataOfSample()
        {
            using (var fileStream = File.OpenRead(this.importedFile))
            {
                return this.archiveUtils.IsZipStream(fileStream)
                    ? this.ParseZip(fileStream)?.FirstOrDefault()
                    : this.ParseText(fileStream, importedFile);
            }
        }

        public PreloadedDataByFile[] GetPreloadedDataOfPanel()
        {
            using (var fileStream = File.OpenRead(this.importedFile))
            {
                return this.ParseZip(fileStream).ToArray();
            }
        }

        public PreloadedDataByFile ParseText(Stream inputStream, string fileName)
        {
            var rows = this.csvReader.ReadRowsWithHeader(inputStream, TabExportFile.Delimiter).ToArray();

            return new PreloadedDataByFile(fileName, rows?.FirstOrDefault(), rows.Skip(1).ToArray());
        }

        public IEnumerable<PreloadedDataByFile> ParseZip(Stream inputStream)
        {
            if (!this.archiveUtils.IsZipStream(inputStream))
                yield break;

            foreach (var file in this.archiveUtils.GetFilesFromArchive(inputStream))
                yield return this.ParseText(new MemoryStream(file.Bytes), file.Name);
        }

        public void DeletePreloadedData()
        {
            if (File.Exists(importedFile))
                File.Delete(importedFile);
        }
    }
}
