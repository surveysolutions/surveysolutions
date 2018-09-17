using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Utils;

namespace WB.Services.Export.Interview.Exporters
{
    internal class InterviewErrorsExporter : IInterviewErrorsExporter
    {
        private readonly ICsvWriter csvWriter;
        private readonly IFileSystemAccessor fileSystemAccessor;
        public const string FileName = "interview__errors";

        private readonly DoExportFileHeader[] errorsFileColumns =
        {
            new DoExportFileHeader("variable", "Variable name for the question, where validation error occurred"),
            new DoExportFileHeader("type", "Type of the variable where the validation error occurred"),
            new DoExportFileHeader("interview__id", "Unique 32-character long identifier of the interview"),
            new DoExportFileHeader("interview__key", "Identifier of the interview"),
            new DoExportFileHeader("message_number", "Numeric index of the validation rule that has fired"),
            new DoExportFileHeader("message", "Text of the error message"),
            new DoExportFileHeader("roster", "Name of the roster containing the variable"),
            new DoExportFileHeader("Id1", "Roster ID of the 1st level of nesting", true),
            new DoExportFileHeader("Id2", "Roster ID of the 2nd level of nesting", true),
            new DoExportFileHeader("Id3", "Roster ID of the 3rd level of nesting", true),
            new DoExportFileHeader("Id4", "Roster ID of the 4th level of nesting", true),
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
            exportRow.Add(error.EntityType == EntityType.Question
                ? questionnaire.GetQuestionVariableName(error.Identity.Id)
                : "");
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
            exportRow.Add(interviewKey);

            for (int i = 0; i < maxRosterDepthInQuestionnaire; i++)
            {
                exportRow.Add(error.Identity.RosterVector.Length > i ? error.Identity.RosterVector[i].ToString() : "");
            }

            exportRow.Add((failedValidationConditionIndex + 1).ToString());
            exportRow.Add(questionnaire.GetValidationMessage(error.Identity.Id, failedValidationConditionIndex).RemoveHtmlTags());
            return exportRow.ToArray();
        }

        public void WriteHeader(bool hasAtLeastOneRoster, int maxRosterDepthInQuestionnaire, string filePath)
        {
            var headers = GetHeaders(hasAtLeastOneRoster, maxRosterDepthInQuestionnaire);

            this.csvWriter.WriteData(filePath, new[] { headers.ToArray() }, ExportFileSettings.DataFileSeparator.ToString());
        }

        public void WriteDoFile(QuestionnaireExportStructure questionnaireExportStructure, string basePath)
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
                    if (exportFileHeader.AddCapture)
                        doContent.AppendCaptureLabelToVariableMatching(exportFileHeader.Title, exportFileHeader.Description);
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
            var headers = new List<string> {"variable", "type"};
            if (hasAtLeastOneRoster)
                headers.Add("roster");

            headers.Add("interview__id");
            headers.Add("interview__key");

            for (int i = 1; i <= maxRosterDepthInQuestionnaire; i++)
            {
                headers.Add($"id{i}");
            }

            headers.Add("message_number");
            headers.Add("message");
            return headers;
        }
    }
}