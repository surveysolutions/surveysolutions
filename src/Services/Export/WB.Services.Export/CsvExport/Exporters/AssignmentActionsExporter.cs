using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WB.Services.Export.Assignment;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.User;

namespace WB.Services.Export.CsvExport.Exporters
{
    public class AssignmentActionsExporter : IAssignmentActionsExporter
    {
        private readonly IOptions<ExportServiceSettings> dataExportSettings;
        public string AssignmentActionsFileName => "assignment__actions";

        public DoExportFileHeader[] ActionFileColumns => new []
        {
            new DoExportFileHeader("assignment__id", "Assignment id (identifier in numeric format)", ExportValueType.NumericInt),
            //new DoExportFileHeader("assignment__key", "Unique 32-character long identifier of the assignment", ExportValueType.String),
            new DoExportFileHeader("date", "Date when the action was taken", ExportValueType.String),
            new DoExportFileHeader("time", "Time when the action was taken", ExportValueType.String),
            new DoExportFileHeader("action", "Type of action taken", ExportValueType.NumericInt, 
                Enum.GetValues(typeof(AssignmentExportedAction))
                    .Cast<AssignmentExportedAction>()
                    .Select(x => new VariableValueLabel(((int)x).ToString(), x.ToString())).ToArray()),
            new DoExportFileHeader("originator", "Login name of the person performing the action", ExportValueType.String),
            new DoExportFileHeader("role", "System role of the person performing the action", ExportValueType.NumericInt,
                ExportHelper.RolesMap
                    .Select(x => new VariableValueLabel(x.Key.ToString(CultureInfo.InvariantCulture), x.Value)).ToArray()),
            new DoExportFileHeader("responsible__name", "Login name of the person now responsible for the assignment", ExportValueType.String),
            new DoExportFileHeader("responsible__role", "System role of the person now responsible for the assignment", ExportValueType.NumericInt,
                ExportHelper.RolesMap
                    .Select(x => new VariableValueLabel(x.Key.ToString(CultureInfo.InvariantCulture), x.Value)).ToArray())
        };

        private readonly string dataFileExtension = "tab";
        private readonly ICsvWriter csvWriter;
        private readonly TenantDbContext dbContext;
        private readonly IUserStorage userStorage;

        public AssignmentActionsExporter(IOptions<ExportServiceSettings> dataExportSettings,
            ICsvWriter csvWriter,
            TenantDbContext dbContext,
            IUserStorage userStorage)
        {
            this.dataExportSettings = dataExportSettings;
            this.csvWriter = csvWriter;
            this.dbContext = dbContext;
            this.userStorage = userStorage;
        }

        public async Task ExportAsync(List<int> assignmentIdsToExport, string basePath, ExportProgress progress, CancellationToken cancellationToken)
        {
            var actionFilePath = Path.Combine(basePath, Path.ChangeExtension(this.AssignmentActionsFileName, this.dataFileExtension));
            var batchSize = this.dataExportSettings.Value.MaxRecordsCountPerOneExportQuery;

            var fileColumns = ActionFileColumns.Select(a => a.Title).ToArray();
            this.csvWriter.WriteData(actionFilePath, new[] { fileColumns }, ExportFileSettings.DataFileSeparator.ToString());

            long totalProcessedCount = 0;

            var batchOptions = new BatchOptions { Max = batchSize };

            foreach (var assignmentsBatch in assignmentIdsToExport.BatchInTime(batchOptions))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var interviewIdsStrings = assignmentsBatch.ToArray();
                var actionsChunk = await this.QueryActionsChunkFromReadSide(interviewIdsStrings);
                cancellationToken.ThrowIfCancellationRequested();
                this.csvWriter.WriteData(actionFilePath, actionsChunk, ExportFileSettings.DataFileSeparator.ToString());

                totalProcessedCount += interviewIdsStrings.Length;
                progress.Report(totalProcessedCount.PercentOf(assignmentIdsToExport.Count));
                cancellationToken.ThrowIfCancellationRequested();
            }

            progress.Report(100);
        }

        public void ExportDoFile(QuestionnaireExportStructure questionnaireExportStructure, string basePath)
        {
            var doContent = new DoFile();

            doContent.BuildInsheet(Path.ChangeExtension(this.AssignmentActionsFileName, this.dataFileExtension));
            doContent.AppendLine();

            foreach (var actionFileColumn in ActionFileColumns)
            {
                if (actionFileColumn.VariableValueLabels.Any())
                {
                    doContent.AppendLabel(actionFileColumn.Title, actionFileColumn.VariableValueLabels);
                    doContent.AppendLabelToValuesMatching(actionFileColumn.Title, actionFileColumn.Title);
                }

                doContent.AppendLabelToVariableMatching(actionFileColumn.Title, actionFileColumn.Description);
            }

            var fileName = $"{AssignmentActionsFileName}.{DoFile.ContentFileNameExtension}";
            var contentFilePath = Path.Combine(basePath, fileName);

            File.WriteAllText(contentFilePath, doContent.ToString());
        }

        private async Task<List<string[]>> QueryActionsChunkFromReadSide(int[] assignmentIds)
        {
            var assignments = dbContext.AssignmentActions.Where(selector => assignmentIds.Contains(selector.Id));
            var result = new List<string[]>();

            foreach (AssignmentAction assignmentAction in assignments)
            {
                var resultRow = new List<string>
                {
                    assignmentAction.Id.ToString(CultureInfo.InvariantCulture),
                    //assignmentAction.PublicKey.FormatGuid(),
                    assignmentAction.Timestamp.ToString(ExportFormatSettings.ExportDateFormat, CultureInfo.InvariantCulture),
                    assignmentAction.Timestamp.ToString("T", CultureInfo.InvariantCulture),
                    ((int)assignmentAction.Status).ToString(CultureInfo.InvariantCulture),
                    await GetUserNameAsync(assignmentAction.OriginatorId),
                    ExportHelper.GetUserRoleDisplayValue(await GetUserRoleAsync(assignmentAction.OriginatorId)),
                    await GetUserNameAsync(assignmentAction.ResponsibleId),
                    ExportHelper.GetUserRoleDisplayValue(await GetUserRoleAsync(assignmentAction.OriginatorId))
                };
                result.Add(resultRow.ToArray());
            }
            return result;
        }

        private Task<UserRoles> GetUserRoleAsync(Guid userId)
        {
            return userStorage.GetUserRoleAsync(userId);
        }

        private Task<string> GetUserNameAsync(Guid userId)
        {
            return userStorage.GetUserNameAsync(userId);
        }
    }
}
