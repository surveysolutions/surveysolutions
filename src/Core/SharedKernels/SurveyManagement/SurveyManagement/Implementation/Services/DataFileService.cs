using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Services;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services
{
    internal class DataFileService : IDataFileService
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        public DataFileService(IFileSystemAccessor fileSystemAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public Dictionary<Guid, string> CreateCleanedFileNamesForLevels(IDictionary<Guid, string> levels)
        {
            var result = new Dictionary<Guid, string>();
            var createdFileNames = new HashSet<string>();
            foreach (var allegedLevelName in levels)
            {
                string levelFileName = this.CreateValidFileName(allegedLevelName.Value, createdFileNames);
                createdFileNames.Add(levelFileName);
                result.Add(allegedLevelName.Key, levelFileName);
            }
            return result;
        }

        public string CreateValidFileName(string name)
        {
            string fileNameWithoutInvalidFileNameChars = this.fileSystemAccessor.MakeValidFileName(name);
            return new string(fileNameWithoutInvalidFileNameChars.Take(118).ToArray());
        }

        private string CreateValidFileName(string name, HashSet<string> createdFileNames, int i = 0)
        {
            var fileNameShortened = CreateValidFileName(name);
            string fileNameWithNumber = string.Concat(fileNameShortened,
                i == 0 ? (object)string.Empty : i).ToLower();

            return !createdFileNames.Contains(fileNameWithNumber)
                ? fileNameWithNumber
                : this.CreateValidFileName(name, createdFileNames, i + 1);
        }
    }
}
