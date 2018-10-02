using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services.Export;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.Export;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    internal class ReadSideToTabularFormatExportService : ITabularFormatExportService
    {
        private readonly string dataFileExtension = "tab";

        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ICsvWriter csvWriter;
        private readonly ILogger logger;

        private readonly IQuestionnaireExportStructureStorage questionnaireExportStructureStorage;

        public ReadSideToTabularFormatExportService(IFileSystemAccessor fileSystemAccessor,
            ICsvWriter csvWriter, 
            ILogger logger,
            IQuestionnaireExportStructureStorage questionnaireExportStructureStorage)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.csvWriter = csvWriter;
            this.logger = logger;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
        }

        public void CreateHeaderStructureForPreloadingForQuestionnaire(QuestionnaireIdentity questionnaireIdentity, string basePath)
        {
            QuestionnaireExportStructure questionnaireExportStructure =
                this.questionnaireExportStructureStorage.GetQuestionnaireExportStructure(questionnaireIdentity);

            if (questionnaireExportStructure == null)
                return;

            this.CreateDataSchemaForInterviewsInTabular(questionnaireExportStructure, basePath);
        }

        private void CreateDataSchemaForInterviewsInTabular(QuestionnaireExportStructure questionnaireExportStructure, string basePath)
        {
            foreach (var level in questionnaireExportStructure.HeaderToLevelMap.Values)
            {
                var dataByTheLevelFilePath =
                    this.fileSystemAccessor.CombinePath(basePath, Path.ChangeExtension(level.LevelName, this.dataFileExtension));

                var interviewLevelHeader = new List<string> {level.LevelIdColumnName};


                if (level.IsTextListScope)
                {
                    interviewLevelHeader.AddRange(level.ReferencedNames);
                }

                foreach (IExportedHeaderItem headerItem in level.HeaderItems.Values)
                {
                    interviewLevelHeader.AddRange(headerItem.ColumnHeaders.Select(x=> x.Name));
                }

                if (level.LevelScopeVector.Length == 0)
                {
                    interviewLevelHeader.AddRange(ServiceColumns.SystemVariables.Values.Select(systemVariable => systemVariable.VariableExportColumnName));
                }

                interviewLevelHeader.AddRange(questionnaireExportStructure.GetAllParentColumnNamesForLevel(level.LevelScopeVector));

                this.csvWriter.WriteData(dataByTheLevelFilePath, new[] { interviewLevelHeader.ToArray() }, ExportFileSettings.DataFileSeparator.ToString());
            }
        }
    }
}
