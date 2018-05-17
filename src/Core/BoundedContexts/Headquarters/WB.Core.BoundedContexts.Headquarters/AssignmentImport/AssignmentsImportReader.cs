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
    public class AssignmentsImportReader : IAssignmentsImportReader
    {
        private readonly ICsvReader csvReader;
        private readonly IArchiveUtils archiveUtils;

        public AssignmentsImportReader(ICsvReader csvReader, IArchiveUtils archiveUtils)
        {
            this.csvReader = csvReader;
            this.archiveUtils = archiveUtils;
        }
        
        private readonly string[] permittedFileExtensions = { TabExportFile.Extention, TextExportFile.Extension };

        private PreloadingRow ToRow(int rowIndex, ExpandoObject record)
        {
            var cells = new Dictionary<string, List<PreloadingValue>>();
            
            foreach (var kv in record)
            {
                var columnName = kv.Key.ToLower();
                var value = (string) kv.Value;

                if (ServiceColumns.AllSystemVariables.Contains(columnName)) continue;

                var compositeColumnValues = columnName.Split(new[] { ServiceColumns.ColumnDelimiter },
                    StringSplitOptions.RemoveEmptyEntries);

                var variableName = compositeColumnValues[0].ToLower();
                var variableOrCodeOrPropertyName = compositeColumnValues.Length > 1 ? compositeColumnValues[1] : variableName;

                if (columnName == ServiceColumns.InterviewId ||
                    columnName == string.Format(ServiceColumns.IdSuffixFormat, variableName))
                {
                    variableName = columnName;
                    variableOrCodeOrPropertyName = columnName;
                }

                if (!cells.ContainsKey(variableName))
                    cells[variableName] = new List<PreloadingValue>();

                cells[variableName].Add(new PreloadingValue
                {
                    VariableOrCodeOrPropertyName = variableOrCodeOrPropertyName,
                    Row = rowIndex,
                    Column = kv.Key,
                    Value = value.Replace(ExportFormatSettings.MissingStringQuestionValue, string.Empty)
                        .Replace(ExportFormatSettings.MissingNumericQuestionValue, string.Empty)
                        .Replace(ExportFormatSettings.MissingQuantityValue, "-1"),
                });
            }

            return new PreloadingRow
            {
                Cells = cells.Select(x => x.Value.Count == 1 && x.Value.First().Column.ToLower() == x.Value.First().VariableOrCodeOrPropertyName
                    ? x.Value[0]
                    : (PreloadingCell) new PreloadingCompositeValue
                    {
                        VariableOrCodeOrPropertyName = x.Key,
                        Values = x.Value.ToArray()
                    }).ToArray()
            };
        }

        public PreloadedFileInfo ReadTextFileInfo(Stream inputStream, string fileName) => new PreloadedFileInfo
        {
            FileName = fileName,
            QuestionnaireOrRosterName = Path.GetFileNameWithoutExtension(fileName),
            Columns = this.csvReader.ReadHeader(inputStream, TabExportFile.Delimiter),
        };

        public PreloadedFile ReadTextFile(Stream inputStream, string fileName) => new PreloadedFile
        {
            FileInfo = new PreloadedFileInfo
            {
                FileName = fileName,
                QuestionnaireOrRosterName = Path.GetFileNameWithoutExtension(fileName),
                Columns = this.csvReader.ReadHeader(inputStream, TabExportFile.Delimiter),
            },
            Rows = this.csvReader.GetRecords(inputStream, TabExportFile.Delimiter)
                .Select((record, rowIndex) => 
                    fileName.Equals($"{ServiceFiles.ProtectedVariables}.tab", StringComparison.OrdinalIgnoreCase) ?
                        (PreloadingRow)this.ToProtectedVariablesRow(rowIndex + 1, record) : 
                        (PreloadingRow)this.ToRow(rowIndex + 1, record)).ToArray()
        };

        private PreloadingRow ToProtectedVariablesRow(int rowIndex, ExpandoObject record)
        {
            return new PreloadingRow
            {
                Cells = record.Where(x => x.Key.Equals(ServiceColumns.ProtectedVariableNameColumn, StringComparison.OrdinalIgnoreCase))
                    .Select(x => new PreloadingValue
                    {
                        VariableOrCodeOrPropertyName = x.Key.ToLower(),
                        Column = x.Key,
                        Row = rowIndex,
                        Value = (string) x.Value
                    }).ToArray()
            };
        }

        public IEnumerable<PreloadedFile> ReadZipFile(Stream inputStream)
        {
            if(!this.archiveUtils.IsZipStream(inputStream))
                yield break;

            foreach (var file in this.archiveUtils.GetFilesFromArchive(inputStream))
            {
                var allowedExtension = permittedFileExtensions.Contains(Path.GetExtension(file.Name));
                var isSystemFile = ServiceFiles.AllSystemFiles.Contains(Path.GetFileNameWithoutExtension(file.Name));

                if (allowedExtension && !isSystemFile)
                    yield return this.ReadTextFile(new MemoryStream(file.Bytes), file.Name);
            }
        }

        public PreloadedFile ReadFileFromZip(Stream inputStream, string fileName)
        {
            if (!this.archiveUtils.IsZipStream(inputStream)) return null;

            foreach (var file in this.archiveUtils.GetFilesFromArchive(inputStream))
            {
                var allowedExtension = permittedFileExtensions.Contains(Path.GetExtension(file.Name));
                var isSystemFile = ServiceFiles.AllSystemFiles.Contains(Path.GetFileNameWithoutExtension(file.Name));

                if (allowedExtension && !isSystemFile && string.Equals(fileName, file.Name, StringComparison.CurrentCultureIgnoreCase))
                    return this.ReadTextFile(new MemoryStream(file.Bytes), file.Name);
            }

            return null;
        }

        public IEnumerable<PreloadedFileInfo> ReadZipFileInfo(Stream inputStream)
        {
            if (!this.archiveUtils.IsZipStream(inputStream))
                yield break;

            foreach (var file in this.archiveUtils.GetFilesFromArchive(inputStream))
            {
                var allowedExtension = permittedFileExtensions.Contains(Path.GetExtension(file.Name));
                var isSystemFile = ServiceFiles.AllSystemFiles.Contains(Path.GetFileNameWithoutExtension(file.Name));

                if (allowedExtension && !isSystemFile && !Path.GetFileNameWithoutExtension(file.Name).Equals(ServiceFiles.ProtectedVariables))
                    yield return this.ReadTextFileInfo(new MemoryStream(file.Bytes), file.Name);
            }
        }
    }
}
