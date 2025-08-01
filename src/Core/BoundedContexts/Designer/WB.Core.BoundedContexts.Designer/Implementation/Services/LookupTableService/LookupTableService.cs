﻿using System;
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
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService
{
    internal class LookupTableService : ILookupTableService
    {
        private readonly IPlainKeyValueStorage<LookupTableContent> lookupTableContentStorage;
        private readonly IDesignerQuestionnaireStorage documentStorage;
        private static readonly Regex VariableNameRegex = new Regex("^[A-Za-z][_A-Za-z0-9]*(?<!_)$", RegexOptions.Compiled);
        private const string ROWCODE = "rowcode";
        private const string DELIMETER = "\t";
        private const int MAX_ROWS_COUNT = 15000;
        private const int MAX_COLS_COUNT = 11;

        public LookupTableService(IPlainKeyValueStorage<LookupTableContent> lookupTableContentStorage,
            IDesignerQuestionnaireStorage documentStorage)
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

            var questionnaire = this.documentStorage.Get(questionnaireId);
            if (questionnaire == null)
                return;

            foreach (var questionnaireLookupTable in questionnaire.LookupTables)
            {
                var lookupTableStorageId = this.GetLookupTableStorageId(questionnaireId, questionnaireLookupTable.Key);

                lookupTableContentStorage.Remove(lookupTableStorageId);
            }
        }

        public LookupTableContent? GetLookupTableContent(Guid questionnaireId, Guid lookupTableId)
        {
            var questionnaire = this.documentStorage.Get(questionnaireId);
            if (questionnaire == null)
                throw new ArgumentException(string.Format(ExceptionMessages.QuestionnaireCantBeFound, questionnaireId));
            
            return GetLookupTableContent(questionnaire, lookupTableId);
        }

        private LookupTableContent? GetLookupTableContent(QuestionnaireDocument questionnaire, Guid lookupTableId)
        {
            if (questionnaire == null)
                throw new ArgumentNullException(nameof(questionnaire));

            if (!questionnaire.LookupTables.TryGetValue(lookupTableId, out var lookupTable))
                throw new ArgumentException(string.Format(ExceptionMessages.LookupTableIsMissing, lookupTableId));

            if (lookupTable == null)
                throw new ArgumentException(string.Format(ExceptionMessages.LookupTableHasEmptyContent, lookupTableId));

            var lookupTableStorageId = this.GetLookupTableStorageId(questionnaire.PublicKey, lookupTableId);

            var lookupTableContent = this.lookupTableContentStorage.GetById(lookupTableStorageId);
            /*if (lookupTableContent == null)
                throw new ArgumentException(string.Format(ExceptionMessages.LookupTableHasEmptyContent, questionnaireId));*/

            return lookupTableContent;
        }

        public LookupTableContentFile? GetLookupTableContentFile(QuestionnaireRevision questionnaireId, Guid lookupTableId)
        {
            var questionnaire = this.documentStorage.Get(questionnaireId);

            if (questionnaire == null)
                throw new ArgumentException(string.Format(ExceptionMessages.QuestionCannotBeFound, questionnaireId));

            return GetLookupTableContentFileImpl(questionnaire, lookupTableId);
        }

        public Dictionary<Guid, string> GetQuestionnairesLookupTables(Guid questionnaireId)
        {
            var questionnaire = this.documentStorage.Get(questionnaireId);
            if (questionnaire == null)
                throw new ArgumentException(string.Format(ExceptionMessages.QuestionCannotBeFound, questionnaireId));

            return GetQuestionnairesLookupTablesImpl(questionnaire);
        }

        public Dictionary<Guid, string> GetQuestionnairesLookupTables(QuestionnaireRevision questionnaireId)
        {
            var questionnaire = this.documentStorage.Get(questionnaireId);
            if (questionnaire == null)
                throw new ArgumentException(string.Format(ExceptionMessages.QuestionCannotBeFound, questionnaireId));

            return GetQuestionnairesLookupTablesImpl(questionnaire);
        }

        private Dictionary<Guid, string> GetQuestionnairesLookupTablesImpl(QuestionnaireDocument questionnaire)
        {

            var result = new Dictionary<Guid, string>();

            foreach (var lookupTable in questionnaire.LookupTables)
            {
                var lookupFile = this.GetLookupTableContentFileImpl(questionnaire, lookupTable.Key);
                if (lookupFile != null)
                    result.Add(lookupTable.Key, System.Text.Encoding.UTF8.GetString(lookupFile.Content));
            }

            return result;
        }

        public void CloneLookupTable(Guid sourceQuestionnaireId, Guid sourceTableId, Guid newQuestionnaireId, Guid newLookupTableId)
        {
            var sourceLookupTableStorageId = this.GetLookupTableStorageId(sourceQuestionnaireId, sourceTableId);
            var content = this.lookupTableContentStorage.GetById(sourceLookupTableStorageId);
            if (content == null)
                throw new InvalidOperationException("Lookup table is empty.");

            var newLookupTableStorageId = this.GetLookupTableStorageId(newQuestionnaireId, newLookupTableId);

            this.lookupTableContentStorage.Store(content, newLookupTableStorageId);
        }

        public bool IsLookupTableEmpty(Guid questionnaireId, Guid tableId, string? lookupTableName)
        {
            if (string.IsNullOrWhiteSpace(lookupTableName))
            {
                return true;
            }

            var lookupTableStorageId = this.GetLookupTableStorageId(questionnaireId, tableId);

            return this.lookupTableContentStorage.GetById(lookupTableStorageId) == null;
        }

        private LookupTableContentFile? GetLookupTableContentFileImpl(QuestionnaireDocument questionnaire, Guid lookupTableId)
        {
            var lookupTableContent = GetLookupTableContent(questionnaire, lookupTableId);

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
                                csvWriter.WriteField(variable.Value.ToString(CultureInfo.InvariantCulture));
                            else
                                csvWriter.WriteField(string.Empty);
                        }
                        csvWriter.NextRecord();
                    }
                }

                return new LookupTableContentFile(
                    content : memoryStream.ToArray(),
                    fileName : questionnaire.LookupTables[lookupTableId].FileName
               );
            }
        }

        private string GetLookupTableStorageId(Guid questionnaireId, Guid lookupTableId)
        {
            return $"{questionnaireId.FormatGuid()}-{lookupTableId.FormatGuid()}";
        }

        private LookupTableContent CreateLookupTableContent(string fileContent)
        {
            using (var csvReader = new CsvParser(new StringReader(fileContent), this.CreateCsvConfiguration()))
            {
                var rows = new List<LookupTableRow>();

                if (!csvReader.Read())
                {
                    throw new ArgumentException(ExceptionMessages.LookupTables_cant_has_empty_content);
                }

                var fieldHeaders = csvReader.Record?.Select(x => x.Trim()).ToArray();
                if (fieldHeaders == null)
                {
                    throw new ArgumentException(ExceptionMessages.LookupTables_cant_has_empty_content);
                }

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

                if (!csvReader.Read())
                {
                    throw new ArgumentException(ExceptionMessages.LookupTables_cant_has_empty_content);
                }
                
                do
                {
                    var variables = new List<decimal?>();
                    var row = new LookupTableRow();
                    var record = csvReader.Record;
                    
                    if (record == null)
                        continue;

                    for (int i = 0; i < amountOfHeaders; i++)
                    {
                        if (i == indexOfRowcodeColumn)
                        {
                            if (!decimal.TryParse(record[i], CultureInfo.InvariantCulture, out var rowCodeAsDecimal))
                            {
                                throw new ArgumentException(string.Format(ExceptionMessages.LookupTables_rowcode_value_cannot_be_parsed, record[i], ROWCODE, rowCurrentRowNumber));
                            }
                            if (rowCodeAsDecimal > long.MaxValue || rowCodeAsDecimal < long.MinValue)
                            {
                                throw new ArgumentException(string.Format(ExceptionMessages.LookupTables_rowcode_value_cannot_be_parsed, record[i], ROWCODE, rowCurrentRowNumber));
                            }
                            
                            long rowCode = (long)rowCodeAsDecimal;
                            if (rowCode != rowCodeAsDecimal)
                            {
                                throw new ArgumentException(string.Format(ExceptionMessages.LookupTables_rowcode_value_cannot_be_parsed, record[i], ROWCODE, rowCurrentRowNumber));
                            }
                            row.RowCode = rowCode;
                        }
                        else
                        {
                            if (i >= record.Length || string.IsNullOrWhiteSpace(record[i]))
                            {
                                variables.Add(null);
                            }
                            else
                            {
                                if (!decimal.TryParse(record[i], NumberStyles.Float, CultureInfo.InvariantCulture, out var variable))
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
                        int rowsCount = rows.Count;
                        do
                        {
                            rowsCount++;
                        } while (csvReader.Read());

                        throw new ArgumentException(string.Format(ExceptionMessages.LookupTables_too_many_rows,
                            $"{MAX_ROWS_COUNT:n0}",
                            $"{rowsCount - 1:n0}"));
                    }
                } while (csvReader.Read());

                var countOfDistinctRowcodeValues = rows.Select(x => x.RowCode).Distinct().Count();

                if (countOfDistinctRowcodeValues != rows.Count())
                {
                    throw new ArgumentException(ExceptionMessages.LookupTables_rowcode_values_must_be_unique);
                }

                return new LookupTableContent(
                    fieldHeaders.Where(h => !h.Equals(ROWCODE, StringComparison.InvariantCultureIgnoreCase)).ToArray(),
                     rows.ToArray());
            }
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
            return new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                Mode = CsvMode.RFC4180,
                Delimiter = DELIMETER,
                MissingFieldFound = null
            };
        }
    }
}
