using System.IO;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Repositories
{
    internal class FilebasedPreloadedDataRepository : IPreloadedDataRepository
    {
        private readonly IArchiveUtils archiveUtils;
        private readonly IAssignmentsImportService assignmentsImportService;
        private readonly string importedFile;

        public FilebasedPreloadedDataRepository(string folderPath, IArchiveUtils archiveUtils,
            IAssignmentsImportService assignmentsImportService)
        {
            this.archiveUtils = archiveUtils;
            this.assignmentsImportService = assignmentsImportService;

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
            throw new System.NotImplementedException();
        }

        public PreloadedDataByFile[] GetPreloadedDataOfPanel()
        {
            throw new System.NotImplementedException();
        }

        public PreloadedFile GetPreloadedDataOfSample1()
        {
            using (var fileStream = File.OpenRead(this.importedFile))
            {
                return this.archiveUtils.IsZipStream(fileStream)
                    ? this.assignmentsImportService.ParseZip(fileStream)?.FirstOrDefault()
                    : this.assignmentsImportService.ParseText(fileStream, importedFile);
            }
        }

        public PreloadedFile[] GetPreloadedDataOfPanel1()
        {
            using (var fileStream = File.OpenRead(this.importedFile))
            {
                return this.assignmentsImportService.ParseZip(fileStream).ToArray();
            }
        }

        public void DeletePreloadedData()
        {
            if (File.Exists(importedFile))
                File.Delete(importedFile);
        }
    }
}
