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

        private static readonly string[] ignoredPreloadingColumns =
            ServiceColumns.SystemVariables.Values.Select(x => x.VariableExportColumnName).ToArray();

        private readonly string[] permittedFileExtensions = { TabExportFile.Extention, TextExportFile.Extension };

        private PreloadingRow ToRow(int rowIndex, ExpandoObject record)
        {
            var cells = new Dictionary<string, List<PreloadingValue>>();
            
            foreach (var kv in record)
            {
                var columnName = kv.Key.ToLower();
                var value = kv.Value?.ToString()?.Replace(ExportFormatSettings.MissingStringQuestionValue, string.Empty)
                    .Replace(ExportFormatSettings.MissingNumericQuestionValue, string.Empty)
                    .Replace(ExportFormatSettings.MissingQuantityValue, string.Empty);

                if(string.IsNullOrEmpty(value)) continue;
                if (ignoredPreloadingColumns.Contains(columnName)) continue;

                var compositeColumnValues = columnName.Split(new[] { ServiceColumns.ColumnDelimiter },
                    StringSplitOptions.RemoveEmptyEntries);

                var variableName = compositeColumnValues[0].ToLower();

                if (!cells.ContainsKey(variableName))
                    cells[variableName] = new List<PreloadingValue>();

                cells[variableName].Add(new PreloadingValue
                {
                    VariableOrCodeOrPropertyName = compositeColumnValues.Length > 1 ? compositeColumnValues[1] : variableName,
                    Row = rowIndex,
                    Column = kv.Key,
                    Value = value
                });
            }

            return new PreloadingRow
            {
                Cells = cells.Select(x => x.Value.Count == 1
                    ? x.Value[0]
                    : (PreloadingCell) new PreloadingCompositeValue
                    {
                        VariableOrCodeOrPropertyName = x.Key.ToLower(),
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
                .Select((record, rowIndex) => (PreloadingRow)this.ToRow(rowIndex + 1, record)).ToArray()
        };

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

                if (allowedExtension && !isSystemFile)
                    yield return this.ReadTextFileInfo(new MemoryStream(file.Bytes), file.Name);
            }
        }
    }
}
