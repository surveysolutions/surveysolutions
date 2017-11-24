using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.Export;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Sanitizer;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters
{
    public interface IInterviewErrorsExporter
    {
        List<string[]> Export(QuestionnaireExportStructure exportStructure, List<InterviewEntity> entitiesToExport, string basePath);
        void WriteHeader(bool hasAtLeastOneRoster, int maxRosterDepthInQuestionnaire, string filePath);
    }

    public class InterviewErrorsExporter : IInterviewErrorsExporter
    {
        private readonly ICsvWriter csvWriter;
        private readonly IQuestionnaireStorage questionnaireStorage;
        public const string FileName = "interview__errors";

        public InterviewErrorsExporter(ICsvWriter csvWriter,
            IQuestionnaireStorage questionnaireStorage)
        {
            this.csvWriter = csvWriter ?? throw new ArgumentNullException(nameof(csvWriter));
            this.questionnaireStorage = questionnaireStorage ?? throw new ArgumentNullException(nameof(questionnaireStorage));
        }

        public List<string[]> Export(QuestionnaireExportStructure exportStructure, List<InterviewEntity> entitiesToExport, string basePath)
        {
            var questionnaire = questionnaireStorage.GetQuestionnaire(exportStructure.Identity, null);
            List<string[]> exportRecords = new List<string[]>();

            foreach (var interviewEntity in entitiesToExport.Where(x => (x.EntityType == EntityType.Question  || x.EntityType == EntityType.StaticText) && x.InvalidValidations?.Length > 0))
            {
                foreach (var failedValidationConditionIndex in interviewEntity.InvalidValidations)
                {
                    string[] exportRow = CreateExportRow(questionnaire, interviewEntity, exportStructure.MaxRosterDepth, failedValidationConditionIndex);
                    exportRecords.Add(exportRow);
                }
            }

            return exportRecords;
        }

        private static string[] CreateExportRow(IQuestionnaire questionnaire, InterviewEntity error,
            int maxRosterDepthInQuestionnaire, int failedValidationConditionIndex)
        {
            List<string> exportRow = new List<string>();
            if (error.EntityType == EntityType.Question)
            {
                exportRow.Add(questionnaire.GetQuestionVariableName(error.Identity.Id));
            }
            else
            {
                exportRow.Add("");
            }
            exportRow.Add(error.EntityType.ToString());

            if (error.Identity.RosterVector.Length > 0)
            {
                var parentRosters = questionnaire.GetRostersFromTopToSpecifiedEntity(error.Identity.Id);
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
                if (error.Identity.RosterVector.Length > i)
                {
                    exportRow.Add(error.Identity.RosterVector[i].ToString());
                }
                else 
                {
                    exportRow.Add("");
                }
            }
            exportRow.Add((failedValidationConditionIndex + 1).ToString());
            exportRow.Add(questionnaire.GetValidationMessage(error.Identity.Id, failedValidationConditionIndex).RemoveHtmlTags());
            return exportRow.ToArray();
        }

        public void WriteHeader(bool hasAtLeastOneRoster, int maxRosterDepthInQuestionnaire, string filePath)
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