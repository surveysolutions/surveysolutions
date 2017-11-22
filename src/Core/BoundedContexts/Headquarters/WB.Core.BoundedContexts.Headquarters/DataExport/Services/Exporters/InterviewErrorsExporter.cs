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
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Sanitizer;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters
{
    public class InterviewErrorsExporter
    {
        private readonly IInterviewFactory interviewFactory;
        private readonly ILogger logger;
        private readonly ICsvWriter csvWriter;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly ITransactionManagerProvider transactionManager;
        private const string FileName = "interview__errors.tab";

        protected InterviewErrorsExporter()
        {
        }

        public InterviewErrorsExporter(IInterviewFactory interviewFactory,
            ILogger logger,
            ICsvWriter csvWriter,
            IQuestionnaireStorage questionnaireStorage,
            ITransactionManagerProvider transactionManager)
        {
            this.interviewFactory = interviewFactory;
            this.logger = logger;
            this.csvWriter = csvWriter;
            this.questionnaireStorage = questionnaireStorage;
            this.transactionManager = transactionManager;
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

                var interveiws = interviewsBatch.ToList();
                var exportedErrors =
                    this.transactionManager.GetTransactionManager().ExecuteInQueryTransaction(() => this.interviewFactory.GetErrors(interveiws));
                List<string[]> exportRecords = new List<string[]>();
                foreach (var error in exportedErrors)
                {
                    foreach (var failedValidationConditionIndex in error.FailedValidationConditions)
                    {
                        string[] exportRow = CreateExportRow(questionnaire, error, maxRosterDepthInQuestionnaire, failedValidationConditionIndex);
                        exportRecords.Add(exportRow);
                    }
                }

                if (exportRecords.Count > 0)
                {
                    this.csvWriter.WriteData(filePath, exportRecords, ExportFileSettings.DataFileSeparator.ToString());
                }
                totalProcessed += interveiws.Count;
                progress.Report(totalProcessed.PercentOf(interviewIdsToExport.Count));
                this.logger.Info($"Exported errors for batch. Processed {totalProcessed} of {interviewIdsToExport.Count}");
            }

            progress.Report(100);
        }

        private static string[] CreateExportRow(IQuestionnaire questionnaire, ExportedError error,
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
            return exportRow.ToArray();
        }

        private void WriteHeader(bool hasAtLeastOneRoster, int maxRosterDepthInQuestionnaire, string filePath)
        {
            var header = new List<string> { "variable", "type" };
            if (hasAtLeastOneRoster)
                header.Add("roster");

            header.Add("interviewid");

            for (int i = 1; i <= maxRosterDepthInQuestionnaire; i++)
            {
                header.Add($"id{i}");
            }
            header.Add("message_number");
            header.Add("message");

            this.csvWriter.WriteData(filePath, new[] { header.ToArray() }, ExportFileSettings.DataFileSeparator.ToString());
        }
    }
}