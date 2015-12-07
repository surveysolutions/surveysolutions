using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService
{
    internal class LookupTableService: ILookupTableService
    {
        private readonly IPlainKeyValueStorage<LookupTableContent> lookupTableContentStorage;
        private readonly IReadSideKeyValueStorage<QuestionnaireDocument> documentStorage;
        private const string ROWCODE = "rowcode";
        private const string DELIMETER = "\t";

        public LookupTableService(IPlainKeyValueStorage<LookupTableContent> lookupTableContentStorage, IReadSideKeyValueStorage<QuestionnaireDocument> documentStorage)
        {
            this.lookupTableContentStorage = lookupTableContentStorage;
            this.documentStorage = documentStorage;
        }

        public void SaveLookupTableContent(Guid questionnaireId, Guid lookupTableId, string lookupTableName, string fileContent)
        {
            DeleteLookupTableContent(questionnaireId, lookupTableId);

            this.lookupTableContentStorage.Store(CreateLookupTableContent(fileContent), GetLookupTableStorageId(questionnaireId, lookupTableName));
        }

        public void DeleteLookupTableContent(Guid questionnaireId, Guid lookupTableId)
        {
            var questionnaire = this.documentStorage.GetById(questionnaireId);

            if (questionnaire == null)
                return;

            if (!questionnaire.LookupTables.ContainsKey(lookupTableId))
                return;

            var lookupTable = questionnaire.LookupTables[lookupTableId];
            if (lookupTable==null)
                return;

            var lookupTableStorageId = GetLookupTableStorageId(questionnaireId, lookupTable.TableName);

            lookupTableContentStorage.Remove(lookupTableStorageId);
        }

        public LookupTableContentFile GetLookupTableContentFile(Guid questionnaireId, Guid lookupTableId)
        {
            var questionnaire = this.documentStorage.GetById(questionnaireId);

            if (questionnaire == null)
                throw new ArgumentException($"questionnaire with id {questionnaireId} is missing");

            if (!questionnaire.LookupTables.ContainsKey(lookupTableId))
                throw new ArgumentException($"lookup table with id {lookupTableId} is missing");

            var lookupTable = questionnaire.LookupTables[lookupTableId];
            if (lookupTable == null)
                throw new ArgumentException($"lookup table with id {lookupTableId} doen't have content");

            var lookupTableStorageId = GetLookupTableStorageId(questionnaireId, lookupTable.TableName);

            var lookupTableContent = this.lookupTableContentStorage.GetById(lookupTableStorageId);

            if (lookupTableContent == null)
                throw new ArgumentException($"lookup table with id {lookupTableId} doen't have content");

            var sb = new StringBuilder();
            using (var csvWriter = new CsvWriter(new StringWriter(sb), this.CreateCsvConfiguration()))
            {
                csvWriter.WriteField(ROWCODE);
                foreach (var variableName in lookupTableContent.VariableNames)
                {
                    csvWriter.WriteField(variableName);
                }
                csvWriter.NextRecord();
                foreach (var lookupTableRow in lookupTableContent.Rows)
                {
                    csvWriter.WriteField(lookupTableRow.RowCode);
                    foreach (var variable in lookupTableRow.Variables)
                    {
                        csvWriter.WriteField(variable);
                    }
                    csvWriter.NextRecord();
                }
            }

            var memoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memoryStream);
            streamWriter.Write(sb.ToString());
            streamWriter.Flush();
            memoryStream.Position = 0;

            return new LookupTableContentFile() {Content = memoryStream, FileName = lookupTable.FileName};
        }

        private string GetLookupTableStorageId(Guid questionnaireId, string lookupTableName)
        {
            return $"{questionnaireId.FormatGuid()}-{lookupTableName}";
        }

        private LookupTableContent CreateLookupTableContent(string fileContent)
        {
            var result = new LookupTableContent();
            var csvReader = new CsvReader(new StringReader(fileContent), this.CreateCsvConfiguration());
            using (csvReader)
            {
                var rows = new List<LookupTableRow>();
                while (csvReader.Read())
                {
                    var variables = new List<decimal>();
                    var row = new LookupTableRow();
                    var record = csvReader.CurrentRecord;

                    for (int i = 0; i < record.Length; i++)
                    {
                        var columnName = csvReader.FieldHeaders[i];
                        if (columnName.Equals(ROWCODE, StringComparison.InvariantCultureIgnoreCase))
                        {
                            row.RowCode = long.Parse(record[i]);
                        }
                        else
                        {
                            variables.Add(decimal.Parse(record[i]));
                        }
                    }

                    row.Variables = variables.ToArray();
                    rows.Add(row);
                }
                result.VariableNames =
                    csvReader.FieldHeaders.Where(h => !h.Equals(ROWCODE, StringComparison.InvariantCultureIgnoreCase)).ToArray();
                result.Rows = rows.ToArray();
            }
            return result;
        }

        private CsvConfiguration CreateCsvConfiguration()
        {
            return new CsvConfiguration { HasHeaderRecord = true, TrimFields = true, IgnoreQuotes = false, Delimiter = DELIMETER };
        }
    }
}
