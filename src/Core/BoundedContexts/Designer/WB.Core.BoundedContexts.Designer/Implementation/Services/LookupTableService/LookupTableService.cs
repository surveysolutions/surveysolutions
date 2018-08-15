using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService
{
    internal class LookupTableService : ILookupTableService
    {
        private readonly IPlainKeyValueStorage<LookupTableContent> lookupTableContentStorage;
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> documentStorage;
        private static readonly Regex VariableNameRegex = new Regex("^[A-Za-z][_A-Za-z0-9]*(?<!_)$", RegexOptions.Compiled);
        private const string ROWCODE = "rowcode";
        private const string DELIMETER = "\t";
        private const int MAX_ROWS_COUNT = 5000;
        private const int MAX_COLS_COUNT = 11;

        public LookupTableService(IPlainKeyValueStorage<LookupTableContent> lookupTableContentStorage,
            IPlainKeyValueStorage<QuestionnaireDocument> documentStorage)
        {
            this.lookupTableContentStorage = lookupTableContentStorage;
            this.documentStorage = documentStorage;
        }

        public void SaveLookupTableContent(Guid questionnaireId, Guid lookupTableId, string fileContent)
        {
            var lookupTableContent = string.IsNullOrWhiteSpace(fileContent) 
                ? this.GetLookupTableContent(questionnaireId, lookupTableId) 
                : this.CreateLookupTableContent(fileContent);

            if (lookupTableContent == null)
            {
                throw new ArgumentException(string.Format(ExceptionMessages.LookupTables_cant_has_empty_content));
            }

            var lookupTableStorageId = this.GetLookupTableStorageId(questionnaireId, lookupTableId);

            this.lookupTableContentStorage.Store(lookupTableContent, lookupTableStorageId);
        }

        public void DeleteAllByQuestionnaireId(Guid questionnaireId)
        {
            //old instances are stuck

            var questionnaire = this.documentStorage.GetById(questionnaireId.FormatGuid());
            if (questionnaire == null)
                return;

            foreach (var questionnaireLookupTable in questionnaire.LookupTables)
            {
                var lookupTableStorageId = this.GetLookupTableStorageId(questionnaireId, questionnaireLookupTable.Key);

                lookupTableContentStorage.Remove(lookupTableStorageId);
            }
        }

        public LookupTableContent GetLookupTableContent(Guid questionnaireId, Guid lookupTableId)
        {
            var questionnaire = this.documentStorage.GetById(questionnaireId.FormatGuid());
            if (questionnaire == null)
                throw new ArgumentException(string.Format(ExceptionMessages.QuestionCannotBeFound, questionnaireId));

            if (!questionnaire.LookupTables.ContainsKey(lookupTableId))
                throw new ArgumentException(string.Format(ExceptionMessages.LookupTableIsMissing, lookupTableId));

            var lookupTable = questionnaire.LookupTables[lookupTableId];
            if (lookupTable == null)
                throw new ArgumentException(string.Format(ExceptionMessages.LookupTableHasEmptyContent, lookupTableId));

            var lookupTableStorageId = this.GetLookupTableStorageId(questionnaire.PublicKey, lookupTableId);

            var lookupTableContent = this.lookupTableContentStorage.GetById(lookupTableStorageId);

            return lookupTableContent;
        }

        public LookupTableContentFile GetLookupTableContentFile(Guid questionnaireId, Guid lookupTableId)
        {
            var questionnaire = this.documentStorage.GetById(questionnaireId.FormatGuid());

            if (questionnaire == null)
                throw new ArgumentException(string.Format(ExceptionMessages.QuestionCannotBeFound, questionnaireId));

            return GetLookupTableContentFileImpl(questionnaire, lookupTableId);
        }

        public Dictionary<Guid, string> GetQuestionnairesLookupTables(Guid questionnaireId)
        {
            var questionnaire = this.documentStorage.GetById(questionnaireId.FormatGuid());

            if (questionnaire == null)
                throw new ArgumentException(string.Format(ExceptionMessages.QuestionCannotBeFound, questionnaireId));

            var result = new Dictionary<Guid, string>();

            foreach (var lookupTable in questionnaire.LookupTables)
            {
                var lookupFile = this.GetLookupTableContentFileImpl(questionnaire, lookupTable.Key);
                if (lookupFile != null)
                    result.Add(lookupTable.Key, System.Text.Encoding.UTF8.GetString(lookupFile.Content));
            }
            return result;
        }

        public void CloneLookupTable(Guid sourceQuestionnaireId, Guid sourceTableId, string sourceLookupTableName, Guid newQuestionnaireId, Guid newLookupTableId)
        {
            var content = GetLookupTableContent(sourceQuestionnaireId, sourceTableId);

            var lookupTableStorageId = this.GetLookupTableStorageId(newQuestionnaireId, newLookupTableId);

            this.lookupTableContentStorage.Store(content, lookupTableStorageId);
        }

        public bool IsLookupTableEmpty(Guid questionnaireId, Guid tableId, string lookupTableName)
        {
            if (string.IsNullOrWhiteSpace(lookupTableName))
            {
                return true;
            }

            var lookupTableStorageId = this.GetLookupTableStorageId(questionnaireId, tableId);

            return this.lookupTableContentStorage.GetById(lookupTableStorageId) == null;
        }

        private LookupTableContentFile GetLookupTableContentFileImpl(QuestionnaireDocument questionnaire, Guid lookupTableId)
        {
            var lookupTableContent = GetLookupTableContent(questionnaire.PublicKey, lookupTableId);

            if (lookupTableContent == null)
                return null;

            using (var memoryStream = new MemoryStream())
            {
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
                            if (variable.HasValue)
                                csvWriter.WriteField(variable.Value);
                            else
                                csvWriter.WriteField(string.Empty);
                        }
                        csvWriter.NextRecord();
                    }
                }

                return new LookupTableContentFile()
                {
                    Content = memoryStream.ToArray(),
                    FileName = questionnaire.LookupTables[lookupTableId].FileName
                };
            }
        }

        private string GetLookupTableStorageId(Guid questionnaireId, Guid lookupTableId)
        {
            return $"{questionnaireId.FormatGuid()}-{lookupTableId.FormatGuid()}";
        }

        private LookupTableContent CreateLookupTableContent(string fileContent)
        {
            var result = new LookupTableContent();

            using (var csvReader = new CsvReader(new StringReader(fileContent), this.CreateCsvConfiguration()))
            {
                var rows = new List<LookupTableRow>();

                if (!csvReader.Read() | !csvReader.ReadHeader() | !csvReader.Read()) // | - because we need excute all Reads
                {
                    throw new ArgumentException(ExceptionMessages.LookupTables_cant_has_empty_content);
                }

                var fieldHeaders = csvReader.Context.HeaderRecord.Select(x => x.Trim()).ToArray();

                var amountOfHeaders = fieldHeaders.Length;

                if (amountOfHeaders > MAX_COLS_COUNT)
                {
                    throw new ArgumentException(string.Format(ExceptionMessages.LookupTables_too_many_columns, MAX_COLS_COUNT));
                }

                if (fieldHeaders.Any(IsVariableNameInvalid))
                {
                    throw new ArgumentException(ExceptionMessages.LookupTables_empty_or_invalid_header_are_not_allowed);
                }

                if (fieldHeaders.Distinct().Count() != amountOfHeaders)
                {
                    throw new ArgumentException(ExceptionMessages.LookupTables_duplicating_headers_are_not_allowed);
                }

                var indexOfRowcodeColumn = fieldHeaders.Select(x => x.ToLower()).ToList().IndexOf(ROWCODE.ToLower());

                if (indexOfRowcodeColumn < 0)
                {
                    throw new ArgumentException(ExceptionMessages.LookupTables_rowcode_column_is_mandatory);
                }
                int rowCurrentRowNumber = 1;

                do
                {
                    var variables = new List<decimal?>();
                    var row = new LookupTableRow();
                    var record = csvReader.Context.Record;

                    for (int i = 0; i < amountOfHeaders; i++)
                    {
                        if (i == indexOfRowcodeColumn)
                        {
                            decimal rowcodeAsDecumal;
                            if (!decimal.TryParse(record[i], out rowcodeAsDecumal))
                            {
                                throw new ArgumentException(string.Format(ExceptionMessages.LookupTables_rowcode_value_cannot_be_parsed, record[i], ROWCODE, rowCurrentRowNumber));
                            }
                            if (rowcodeAsDecumal > long.MaxValue || rowcodeAsDecumal < long.MinValue)
                            {
                                throw new ArgumentException(string.Format(ExceptionMessages.LookupTables_rowcode_value_cannot_be_parsed, record[i], ROWCODE, rowCurrentRowNumber));
                            }
                            long rowcode = (long)rowcodeAsDecumal;
                            var isParcedDecimalValueConvertableToInteger = (decimal)rowcode == rowcodeAsDecumal;
                            if (!isParcedDecimalValueConvertableToInteger)
                            {
                                throw new ArgumentException(string.Format(ExceptionMessages.LookupTables_rowcode_value_cannot_be_parsed, record[i], ROWCODE, rowCurrentRowNumber));
                            }
                            row.RowCode = rowcode;
                        }
                        else
                        {
                            if (i >= record.Length || string.IsNullOrWhiteSpace(record[i]))
                            {
                                variables.Add(null);
                            }
                            else
                            {
                                decimal variable;
                                if (!decimal.TryParse(record[i], NumberStyles.Float, CultureInfo.InvariantCulture, out variable))
                                {
                                    throw new ArgumentException(string.Format(ExceptionMessages.LookupTables_data_value_cannot_be_parsed, record[i], fieldHeaders[i], rowCurrentRowNumber));
                                }
                                variables.Add(variable);
                            }
                        }
                    }
                    rowCurrentRowNumber++;
                    row.Variables = variables.ToArray();
                    rows.Add(row);
                    if (rows.Count > MAX_ROWS_COUNT)
                    {
                        throw new ArgumentException(string.Format(ExceptionMessages.LookupTables_too_many_rows, MAX_ROWS_COUNT));
                    }
                } while (csvReader.Read());

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

        private Configuration CreateCsvConfiguration()
        {
            return new Configuration { HasHeaderRecord = true, TrimOptions = TrimOptions.Trim, IgnoreQuotes = false, Delimiter = DELIMETER, MissingFieldFound = null };
        }
    }
}
