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

            var lookupTableStoredName = questionnaire.LookupTables[lookupTableId];
            if (string.IsNullOrEmpty(lookupTableStoredName))
                return;

            var lookupTableStorageId = GetLookupTableStorageId(questionnaireId, lookupTableStoredName);

            lookupTableContentStorage.Remove(lookupTableStorageId);
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
                    var row = new LookupTableRow() {Variables = new Dictionary<string, decimal>()};
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
                            row.Variables.Add(columnName, decimal.Parse(record[i]));
                        }
                    }
                    rows.Add(row);
                }
                result.VariableNames =
                    csvReader.FieldHeaders.Where(h => h.Equals(ROWCODE, StringComparison.InvariantCultureIgnoreCase)).ToArray();
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
