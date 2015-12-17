using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using CsQuery.ExtensionMethods;
using CsvHelper;
using CsvHelper.Configuration;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService
{
    internal class LookupTableService : ILookupTableService
    {
        private readonly IPlainKeyValueStorage<LookupTableContent> lookupTableContentStorage;
        private readonly IReadSideKeyValueStorage<QuestionnaireDocument> documentStorage;
        private static readonly Regex VariableNameRegex = new Regex("^[A-Za-z][_A-Za-z0-9]*(?<!_)$");
        private const string ROWCODE = "rowcode";
        private const string DELIMETER = "\t";
        private const int MAX_ROWS_COUNT = 5000;
        private const int MAX_COLS_COUNT = 11;

        public LookupTableService(IPlainKeyValueStorage<LookupTableContent> lookupTableContentStorage, IReadSideKeyValueStorage<QuestionnaireDocument> documentStorage)
        {
            this.lookupTableContentStorage = lookupTableContentStorage;
            this.documentStorage = documentStorage;
        }

        public void SaveLookupTableContent(Guid questionnaireId, Guid lookupTableId, string lookupTableName, string fileContent)
        {
            var lookupTableContent = string.IsNullOrWhiteSpace(fileContent) 
                ? this.GetLookupTableContent(questionnaireId, lookupTableId) 
                : this.CreateLookupTableContent(fileContent);

            if (lookupTableContent == null)
            {
                throw new ArgumentException(string.Format(ExceptionMessages.LookupTables_cant_has_empty_content));
            }

            var lookupTableStorageId = this.GetLookupTableStorageId(questionnaireId, lookupTableName);

            DeleteLookupTableContent(questionnaireId, lookupTableId);

            this.lookupTableContentStorage.Store(lookupTableContent, lookupTableStorageId);
        }

        public void DeleteLookupTableContent(Guid questionnaireId, Guid lookupTableId)
        {
            var questionnaire = this.documentStorage.GetById(questionnaireId);

            if (questionnaire == null)
                return;

            if (!questionnaire.LookupTables.ContainsKey(lookupTableId))
                return;

            var lookupTable = questionnaire.LookupTables[lookupTableId];
            if (lookupTable == null)
                return;

            var lookupTableStorageId = GetLookupTableStorageId(questionnaireId, lookupTable.TableName);

            lookupTableContentStorage.Remove(lookupTableStorageId);
        }

        public LookupTableContent GetLookupTableContent(Guid questionnaireId, Guid lookupTableId)
        {
            var questionnaire = this.documentStorage.GetById(questionnaireId);

            if (questionnaire == null)
                throw new ArgumentException($"questionnaire with id {questionnaireId} is missing");

            if (!questionnaire.LookupTables.ContainsKey(lookupTableId))
                throw new ArgumentException($"lookup table with id {lookupTableId} is missing");

            var lookupTable = questionnaire.LookupTables[lookupTableId];
            if (lookupTable == null)
                throw new ArgumentException($"lookup table with id {lookupTableId} doen't have content");

            var lookupTableStorageId = GetLookupTableStorageId(questionnaire.PublicKey, lookupTable.TableName);

            var lookupTableContent = this.lookupTableContentStorage.GetById(lookupTableStorageId);

            return lookupTableContent;
        }

        public LookupTableContentFile GetLookupTableContentFile(Guid questionnaireId, Guid lookupTableId)
        {
            var questionnaire = this.documentStorage.GetById(questionnaireId);

            if (questionnaire == null)
                throw new ArgumentException($"questionnaire with id {questionnaireId} is missing");

            return GetLookupTableContentFileImpl(questionnaire, lookupTableId);
        }

        public Dictionary<Guid, string> GetQuestionnairesLookupTables(Guid questionnaireId)
        {
            var questionnaire = this.documentStorage.GetById(questionnaireId);

            if (questionnaire == null)
                throw new ArgumentException($"questionnaire with id {questionnaireId} is missing");

            var result = new Dictionary<Guid, string>();

            foreach (var lookupTable in questionnaire.LookupTables)
            {
                result.Add(lookupTable.Key,
                    System.Text.Encoding.UTF8.GetString(
                        this.GetLookupTableContentFileImpl(questionnaire, lookupTable.Key).Content));
            }
            return result;
        }

        public void CloneLookupTable(Guid sourceQuestionnaireId, Guid sourceTableId, string sourceLookupTableName, Guid newQuestionnaireId)
        {
            var content = GetLookupTableContent(sourceQuestionnaireId, sourceTableId);

            var lookupTableStorageId = this.GetLookupTableStorageId(newQuestionnaireId, sourceLookupTableName);

            this.lookupTableContentStorage.Store(content, lookupTableStorageId);
        }

        private LookupTableContentFile GetLookupTableContentFileImpl(QuestionnaireDocument questionnaire, Guid lookupTableId)
        {
            var lookupTableContent = GetLookupTableContent(questionnaire.PublicKey, lookupTableId);

            var memoryStream = new MemoryStream();
            using (var csvWriter = new CsvWriter(new StreamWriter(memoryStream), this.CreateCsvConfiguration()))
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

            return new LookupTableContentFile() { Content = memoryStream.ToArray(), FileName = questionnaire.LookupTables[lookupTableId].FileName };
        }
        private string GetLookupTableStorageId(Guid questionnaireId, string lookupTableName)
        {
            return $"{questionnaireId.FormatGuid()}-{lookupTableName}";
        }

        private LookupTableContent CreateLookupTableContent(string fileContent)
        {
            var result = new LookupTableContent();

            using (var csvReader = new CsvReader(new StringReader(fileContent), this.CreateCsvConfiguration()))
            {
                var rows = new List<LookupTableRow>();

                if (!csvReader.Read())
                {
                    throw new ArgumentException(ExceptionMessages.LookupTables_cant_has_empty_content);
                }

                var fieldHeaders = csvReader.FieldHeaders.Select(x => x.Trim()).ToArray();

                if (fieldHeaders.Length > MAX_COLS_COUNT)
                {
                    throw new ArgumentException(string.Format(ExceptionMessages.LookupTables_too_many_columns, MAX_COLS_COUNT));
                }

                if (fieldHeaders.Any(IsVariableNameInvalid))
                {
                    throw new ArgumentException(ExceptionMessages.LookupTables_empty_or_invalid_header_are_not_allowed);
                }

                if (fieldHeaders.Distinct().Count() != fieldHeaders.Length)
                {
                    throw new ArgumentException(ExceptionMessages.LookupTables_duplicating_headers_are_not_allowed);
                }

                var indexOfRowcodeColumn = fieldHeaders.Select(x => x.ToLower()).IndexOf(ROWCODE.ToLower());

                if (indexOfRowcodeColumn < 0)
                {
                    throw new ArgumentException(ExceptionMessages.LookupTables_rowcode_column_is_mandatory);
                }
                int rowCurrentRowNumber = 1;

                do
                {
                    var variables = new List<decimal?>();
                    var row = new LookupTableRow();
                    var record = csvReader.CurrentRecord;

                    for (int i = 0; i < record.Length; i++)
                    {
                        if (i == indexOfRowcodeColumn)
                        {
                            long rowcode;
                            if (!long.TryParse(record[i], out rowcode))
                            {
                                throw new ArgumentException(string.Format(ExceptionMessages.LookupTables_rowcode_value_cannot_be_parsed, record[i], ROWCODE, rowCurrentRowNumber));
                            }
                            row.RowCode = rowcode;
                        }
                        else
                        {
                            decimal variable;
                            if (!decimal.TryParse(record[i], out variable))
                            {
                                throw new ArgumentException(string.Format(ExceptionMessages.LookupTables_data_value_cannot_be_parsed, record[i], fieldHeaders[i], rowCurrentRowNumber));
                            }
                            variables.Add(variable);
                        }
                    }
                    rowCurrentRowNumber++;
                    row.Variables = variables.ToArray();
                    rows.Add(row);
                    if (rows.Count > MAX_ROWS_COUNT)
                    {
                        throw new ArgumentException(string.Format(ExceptionMessages.LookupTables_too_many_rows, MAX_ROWS_COUNT));
                    }
                }while (csvReader.Read());

                var countOfDistinctRowcodeValues = rows.Select(x => x.RowCode).Distinct().Count();

                if (countOfDistinctRowcodeValues != rows.Count())
                {
                    throw new ArgumentException(ExceptionMessages.LookupTables_rowcode_values_must_be_unique);
                }

                result.VariableNames = fieldHeaders.Where(h => !h.Equals(ROWCODE, StringComparison.InvariantCultureIgnoreCase)).ToArray();
                result.Rows = rows.ToArray();
            }
            return result;
        }

        private static bool IsVariableNameInvalid(string variableName)
        {
            if (string.IsNullOrEmpty(variableName))
                return true;

            if (variableName.Length > 32)
                return true;

            return !VariableNameRegex.IsMatch(variableName);
        }

        private CsvConfiguration CreateCsvConfiguration()
        {
            return new CsvConfiguration { HasHeaderRecord = true, TrimFields = true, IgnoreQuotes = false, Delimiter = DELIMETER };
        }
    }
}
