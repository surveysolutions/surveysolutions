using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading
{
    internal class RosterDataService : IRosterDataService
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        
        public RosterDataService(IFileSystemAccessor fileSystemAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public HeaderStructureForLevel FindLevelInPreloadedData(PreloadedDataByFile levelData, QuestionnaireExportStructure exportStructure)
        {
            return
                exportStructure.HeaderToLevelMap.Values.FirstOrDefault(
                    header => levelData.FileName.Contains(header.LevelName));
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

        private string CreateValidFileName(string name, HashSet<string> createdFileNames, int i = 0)
        {
            string fileNameWithoutInvalidFileNameChars = this.fileSystemAccessor.MakeValidFileName(name);
            var fileNameShortened = new string(fileNameWithoutInvalidFileNameChars.Take(250).ToArray());
            string fileNameWithNumber = string.Concat(fileNameShortened,
                i == 0 ? (object)string.Empty : i).ToLower();

            return !createdFileNames.Contains(fileNameWithNumber)
                ? fileNameWithNumber
                : this.CreateValidFileName(name, createdFileNames, i + 1);
        }
    }
}
