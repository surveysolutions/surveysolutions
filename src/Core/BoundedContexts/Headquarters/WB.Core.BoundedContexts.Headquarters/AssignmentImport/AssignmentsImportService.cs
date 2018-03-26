using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport
{
    public class AssignmentsImportService : IAssignmentsImportService
    {
        private readonly ICsvReader csvReader;
        private readonly IArchiveUtils archiveUtils;
        private readonly string[] permittedFileExtensions = { TabExportFile.Extention, TextExportFile.Extension };

        private static readonly string[] ignoredPreloadingColumns =
            ServiceColumns.SystemVariables.Values.Select(x => x.VariableExportColumnName).ToArray();

        public AssignmentsImportService(ICsvReader csvReader, IArchiveUtils archiveUtils)
        {
            this.csvReader = csvReader;
            this.archiveUtils = archiveUtils;
        }

        private PreloadingRow ToRow(int rowIndex, ExpandoObject record)
        {
            var cells = new Dictionary<string, List<PreloadingValue>>();
            string interviewId = null;

            string GetVariable(string[] compositeColumnValues, string variableName) 
                => compositeColumnValues.Length > 1 && string.Format(ServiceColumns.IdSuffixFormat, variableName) != variableName
                ? compositeColumnValues[1]
                : variableName;

            foreach (var kv in record)
            {
                var variableName = kv.Key.ToLower();
                var value = (string) kv.Value;

                if (ignoredPreloadingColumns.Contains(variableName)) continue;
                if (variableName == ServiceColumns.InterviewId)
                {
                    interviewId = value;
                    continue;
                }

                var compositeColumnValues = kv.Key.Split(new[] { QuestionDataParser.ColumnDelimiter },
                    StringSplitOptions.RemoveEmptyEntries);

                variableName = compositeColumnValues[0].ToLower();

                if (!cells.ContainsKey(variableName))
                    cells[variableName] = new List<PreloadingValue>();

                cells[variableName].Add(new PreloadingValue
                {
                    VariableOrCodeOrPropertyName = GetVariable(compositeColumnValues, variableName),
                    Row = rowIndex,
                    Column = kv.Key,
                    Value = value.Replace(ExportFormatSettings.MissingStringQuestionValue, string.Empty)
                        .Replace(ExportFormatSettings.MissingNumericQuestionValue, string.Empty)
                        .Replace(ExportFormatSettings.MissingQuantityValue, string.Empty),
                });
            }

            return new PreloadingRow
            {
                InterviewId = interviewId,
                Cells = cells.Select(x => x.Value.Count == 1
                    ? x.Value[0]
                    : (PreloadingCell) new PreloadingCompositeValue
                    {
                        VariableOrCodeOrPropertyName = x.Key,
                        Values = x.Value.ToArray()
                    }).ToArray()
            };
        }

        public PreloadedFile ParseText(Stream inputStream, string fileName) => new PreloadedFile
        {
            FileName = fileName,
            QuestionnaireOrRosterName = Path.GetFileNameWithoutExtension(fileName),
            Columns = this.csvReader.ReadHeader(inputStream, TabExportFile.Delimiter),
            Rows = this.csvReader.GetRecords(inputStream, TabExportFile.Delimiter)
                .Select((record, rowIndex) => (PreloadingRow)this.ToRow(rowIndex + 1, record)).ToArray()
        };

        public IEnumerable<PreloadedFile> ParseZip(Stream inputStream)
        {
            if(!this.archiveUtils.IsZipStream(inputStream))
                yield break;
            
            foreach (var file in this.archiveUtils.GetFilesFromArchive(inputStream))
                yield return this.ParseText(new MemoryStream(file.Bytes), file.Name);
        }

        public IEnumerable<PreloadedFileMetaData> ParseZipMetadata(Stream inputStream)
        {
            if (!this.archiveUtils.IsZipStream(inputStream))
                yield break;

            foreach (var fileInfo in this.archiveUtils.GetArchivedFileNamesAndSize(inputStream))
            {
                yield return new PreloadedFileMetaData(fileInfo.Key, fileInfo.Value,
                    permittedFileExtensions.Contains(Path.GetExtension(fileInfo.Key)));
            }
        }
    }
}