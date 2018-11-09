using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Utils;

namespace WB.Services.Export.CsvExport.Exporters
{
    public class InterviewErrorsExporter : IInterviewErrorsExporter
    {
        private readonly ICsvWriter csvWriter;
        private readonly IFileSystemAccessor fileSystemAccessor;
        public const string FileName = "interview__errors";

        private readonly DoExportFileHeader[] errorsFileColumns =
        {
            CommonHeaderItems.InterviewKey,
            CommonHeaderItems.InterviewId,
            CommonHeaderItems.Roster,
            CommonHeaderItems.Id1,
            CommonHeaderItems.Id2,
            CommonHeaderItems.Id3,
            CommonHeaderItems.Id4,
            new DoExportFileHeader("variable", "Variable name for the question, where validation error occurred", ExportValueType.String),
            new DoExportFileHeader("type", "Type of the variable where the validation error occurred", ExportValueType.String),
            new DoExportFileHeader("message__number", "Numeric index of the validation rule that has fired", ExportValueType.String),
            new DoExportFileHeader("message", "Text of the error message", ExportValueType.String)
        };


        public InterviewErrorsExporter(ICsvWriter csvWriter,
            IFileSystemAccessor fileSystemAccessor)
        {
            this.csvWriter = csvWriter ?? throw new ArgumentNullException(nameof(csvWriter));
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public List<string[]> Export(
            QuestionnaireExportStructure exportStructure,
            QuestionnaireDocument questionnaire,
            List<InterviewEntity> entitiesToExport, 
            string path, 
            string interviewKey)
        {
            List<string[]> exportRecords = new List<string[]>();

            foreach (var interviewEntity in entitiesToExport.Where(
                x => (x.EntityType == EntityType.Question  || x.EntityType == EntityType.StaticText) 
                     && x.InvalidValidations?.Length > 0 
                     && x.IsEnabled))
            {
                foreach (var failedValidationConditionIndex in interviewEntity.InvalidValidations)
                {
                    string[] exportRow = CreateExportRow(questionnaire, interviewEntity, exportStructure.MaxRosterDepth, failedValidationConditionIndex, interviewKey);
                    exportRecords.Add(exportRow);
                }
            }

            return exportRecords;
        }

        private static string[] CreateExportRow(QuestionnaireDocument questionnaire, InterviewEntity error,
            int maxRosterDepthInQuestionnaire, int failedValidationConditionIndex, string interviewKey)
        {
            List<string> exportRow = new List<string>();

            exportRow.Add(interviewKey);
            exportRow.Add(error.InterviewId.FormatGuid());
            
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

            for (int i = 0; i < maxRosterDepthInQuestionnaire; i++)
            {
                exportRow.Add(error.Identity.RosterVector.Length > i ? error.Identity.RosterVector[i].ToString() : "");
            }

            exportRow.Add(error.EntityType == EntityType.Question
                ? questionnaire.GetQuestionVariableName(error.Identity.Id)
                : "");
            exportRow.Add(error.EntityType.ToString());

            exportRow.Add((failedValidationConditionIndex + 1).ToString());
            exportRow.Add(questionnaire.GetValidationMessage(error.Identity.Id, failedValidationConditionIndex).RemoveHtmlTags());
            return exportRow.ToArray();
        }

        public void WriteHeader(bool hasAtLeastOneRoster, int maxRosterDepthInQuestionnaire, string filePath)
        {
            var headers = GetHeaders(hasAtLeastOneRoster, maxRosterDepthInQuestionnaire);

            this.csvWriter.WriteData(filePath, new[] { headers.ToArray() }, ExportFileSettings.DataFileSeparator.ToString());
        }

        public void ExportDoFile(QuestionnaireExportStructure questionnaireExportStructure, string basePath)
        {
            var doContent = new DoFile();
            
            doContent.BuildInsheet(Path.ChangeExtension(FileName, "tab"));
            doContent.AppendLine();

            bool hasAtLeastOneRoster = questionnaireExportStructure.HeaderToLevelMap.Values.Any(x => x.LevelScopeVector.Count > 0);
            var maxRosterDepthInQuestionnaire = questionnaireExportStructure.MaxRosterDepth;

            var headersList = GetHeaders(hasAtLeastOneRoster, maxRosterDepthInQuestionnaire);

            foreach (var header in headersList)
            {
                var exportFileHeader = errorsFileColumns.SingleOrDefault(c => c.Title.Equals(header, StringComparison.CurrentCultureIgnoreCase));
                if (exportFileHeader != null)
                {
                    if (exportFileHeader.AddCaption)
                        doContent.AppendCaptionLabelToVariableMatching(exportFileHeader.Title, exportFileHeader.Description);
                    else
                        doContent.AppendLabelToVariableMatching(exportFileHeader.Title, exportFileHeader.Description);
                }
                else
                {
                    doContent.AppendLabelToVariableMatching(header, string.Empty);
                }
            }

            var fileName = $"{FileName}.{DoFile.ContentFileNameExtension}";
            var contentFilePath = Path.Combine(basePath, fileName);

            this.fileSystemAccessor.WriteAllText(contentFilePath, doContent.ToString());
        }

        public DoExportFileHeader[] GetHeader()
        {
            return errorsFileColumns;
        }

        public string GetFileName()
        {
            return FileName;
        }

        private static List<string> GetHeaders(bool hasAtLeastOneRoster, int maxRosterDepthInQuestionnaire)
        {
            var headers = new List<string> { "interview__key", "interview__id"};

            if (hasAtLeastOneRoster)
                headers.Add("roster");

            for (int i = 1; i <= maxRosterDepthInQuestionnaire; i++)
            {
                headers.Add($"id{i}");
            }

            headers.Add("variable");
            headers.Add("type");

            headers.Add("message__number");
            headers.Add("message");
            return headers;
        }
    }
}
