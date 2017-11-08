using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.Export;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Sanitizer;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters
{
    public class InterviewErrorsExporter
    {
        private readonly InterviewsErrorsReader errorsReader;
        private readonly ILogger logger;
        private readonly ICsvWriter csvWriter;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private const string FileName = "interview__errors.tab";

        protected InterviewErrorsExporter()
        {
        }

        public InterviewErrorsExporter(InterviewsErrorsReader errorsReader,
            ILogger logger,
            ICsvWriter csvWriter,
            IQuestionnaireStorage questionnaireStorage)
        {
            this.errorsReader = errorsReader;
            this.logger = logger;
            this.csvWriter = csvWriter;
            this.questionnaireStorage = questionnaireStorage;
        }

        public void Export(QuestionnaireExportStructure exportStructure, List<Guid> interviewIdsToExport, string basePath, IProgress<int> progress, CancellationToken cancellationToken)
        {
            long totalProcessed = 0;
            
            bool hasAtLeastOneRoster = exportStructure.HeaderToLevelMap.Values.Any(x => x.LevelScopeVector.Count > 0);
            int maxRosterDepthInQuestionnaire = exportStructure.HeaderToLevelMap.Values.Max(x => x.LevelScopeVector.Count);

            var filePath = Path.Combine(basePath, FileName);
            WriteHeader(hasAtLeastOneRoster, maxRosterDepthInQuestionnaire, filePath);

            var questionnaire = questionnaireStorage.GetQuestionnaire(
                new QuestionnaireIdentity(exportStructure.QuestionnaireId, exportStructure.Version), null);

            foreach (var interviewsBatch in interviewIdsToExport.Batch(40))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var exportedErrors = this.errorsReader.GetErrors(interviewsBatch.ToList());
                foreach (var error in exportedErrors)
                {
                    foreach (var failedValidationConditionIndex in error.FailedValidationConditions)
                    {
                        var exportRow = ConvertExportRowToCsv(questionnaire, error, maxRosterDepthInQuestionnaire, failedValidationConditionIndex);
                        this.csvWriter.WriteData(filePath, new[] { exportRow.ToArray() }, ExportFileSettings.DataFileSeparator.ToString());
                    }
                }

                progress.Report(totalProcessed.PercentOf(interviewIdsToExport.Count));
                this.logger.Info($"Exported errors for batch. Processed {totalProcessed} of {interviewIdsToExport.Count}");
            }

            progress.Report(100);
        }

        private static List<string> ConvertExportRowToCsv(IQuestionnaire questionnaire, ExportedError error,
            int maxRosterDepthInQuestionnaire, int failedValidationConditionIndex)
        {
            List<string> exportRow = new List<string>();
            if (error.EntityType == EntityType.Question)
            {
                exportRow.Add(questionnaire.GetQuestionVariableName(error.EntityId));
            }
            else
            {
                exportRow.Add("");
            }
            exportRow.Add(error.EntityType.ToString());

            if (error.RosterVector.Length > 0)
            {
                var parentRosters = questionnaire.GetRostersFromTopToSpecifiedEntity(error.EntityId);
                Guid lastRoster = parentRosters.Last();
                var rosterName = questionnaire.GetRosterVariableName(lastRoster);

                exportRow.Add(rosterName);
            }
            else if(maxRosterDepthInQuestionnaire > 0)
            {
                exportRow.Add("");
            }

            exportRow.Add(error.InterviewId.FormatGuid());

            for (int i = 0; i < maxRosterDepthInQuestionnaire; i++)
            {
                if (error.RosterVector.Length > i)
                {
                    exportRow.Add(error.RosterVector[i].ToString());
                }
                else 
                {
                    exportRow.Add("");
                }
            }
            exportRow.Add((failedValidationConditionIndex + 1).ToString());
            exportRow.Add(questionnaire.GetValidationMessage(error.EntityId, failedValidationConditionIndex).RemoveHtmlTags());
            return exportRow;
        }

        private void WriteHeader(bool hasAtLeastOneRoster, int maxRosterDepthInQuestionnaire, string filePath)
        {
            var header = new List<string> { "Variable", "Type" };
            if (hasAtLeastOneRoster)
                header.Add("Roster");

            header.Add("InterviewId");

            for (int i = 1; i <= maxRosterDepthInQuestionnaire; i++)
            {
                header.Add($"Id{i}");
            }
            header.Add("Message Number");
            header.Add("Message");

            this.csvWriter.WriteData(filePath, new[] { header.ToArray() }, ExportFileSettings.DataFileSeparator.ToString());
        }
    }
}