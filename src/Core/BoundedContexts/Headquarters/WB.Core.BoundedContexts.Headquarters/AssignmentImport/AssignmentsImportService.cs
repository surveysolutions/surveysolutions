using System.Collections.Generic;
using System.IO;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport
{
    public class AssignmentsImportService : IAssignmentsImportService
    {
        private readonly ICsvReader csvReader;
        private readonly IArchiveUtils archiveUtils;
        private readonly string[] permittedFileExtensions = { TabExportFile.Extention, TextExportFile.Extension };

        public AssignmentsImportService(ICsvReader csvReader, IArchiveUtils archiveUtils)
        {
            this.csvReader = csvReader;
            this.archiveUtils = archiveUtils;
        }

        public PreloadedDataByFile ParseText(Stream inputStream, string fileName)
        {
            var rows = this.csvReader.ReadRowsWithHeader(inputStream, TabExportFile.Delimiter).ToArray();

            return new PreloadedDataByFile(fileName, rows?.FirstOrDefault(), rows.Skip(1).ToArray());
        }

        public IEnumerable<PreloadedDataByFile> ParseZip(Stream inputStream)
        {
            if(!this.archiveUtils.IsZipStream(inputStream))
                yield break;
            
            foreach (var file in this.archiveUtils.GetFilesFromArchive(inputStream))
                yield return this.ParseText(new MemoryStream(file.Bytes), file.Name);
        }

        public IEnumerable<PreloadedFileMetaData> ParseZipMetadata(Stream inputStream)
        {
            if (!this.archiveUtils.IsZipStream(inputStream))
                yield break;

            foreach (var fileInfo in this.archiveUtils.GetArchivedFileNamesAndSize(inputStream))
            {
                yield return new PreloadedFileMetaData(fileInfo.Key, fileInfo.Value,
                    permittedFileExtensions.Contains(Path.GetExtension(fileInfo.Key)));
            }
        }
    }
}