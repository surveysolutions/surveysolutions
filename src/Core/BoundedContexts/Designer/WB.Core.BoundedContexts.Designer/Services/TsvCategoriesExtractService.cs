using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.Verifier;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Designer.Services
{
    internal class TsvCategoriesExtractService : ICategoriesExtractService
    {
        private readonly ICategoriesVerifier verifier;

        public TsvCategoriesExtractService(ICategoriesVerifier verifier)
        {
            this.verifier = verifier;
        }

        private static CsvConfiguration CreateCsvConfiguration() => new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,
            TrimOptions = TrimOptions.Trim,
            IgnoreQuotes = false,
            Delimiter = "\t"

        };

        public List<CategoriesRow> Extract(Stream file)
        {
            var categories = new List<CategoriesRow>();
            var errors = new List<ImportValidationError>();
            var headers = new CategoriesHeaderMap
            {
                IdIndex = "0",
                TextIndex = "1",
                ParentIdIndex = "2"
            };

            using (var csvReader = new CsvParser(new StreamReader(file), CreateCsvConfiguration()))
            {
                var rawRow = csvReader.Read()?.ToList();
                var headersFromFile = TryGetHeadersFromFile(rawRow);
                if (headersFromFile != null)
                {
                    headers = headersFromFile;

                    // if file with headers move reader to next line
                    rawRow = csvReader.Read()?.ToList();
                }

                while (true)
                {
                    if (rawRow == null) break;

                    var row = GetRowValues(rawRow, headers, csvReader.Context.RawRow);

                    if (!string.IsNullOrEmpty(row.Id) || !string.IsNullOrEmpty(row.ParentId) || !string.IsNullOrEmpty(row.Text))
                    {
                        var error = this.verifier.Verify(row, headers);

                        if (error != null) 
                            errors.Add(error);
                        else 
                            categories.Add(row);

                        if (errors.Count >= 10) break;

                        if (categories.Count > AbstractVerifier.MaxOptionsCountInFilteredComboboxQuestion)
                            throw new InvalidFileException(ExceptionMessages.Excel_Categories_More_Than_Limit.FormatString(AbstractVerifier.MaxOptionsCountInFilteredComboboxQuestion));
                    }

                    rawRow = csvReader.Read()?.ToList();
                }
            }

            if (errors.Any())
                throw new InvalidFileException(ExceptionMessages.ProvidedFileHasErrors) {FoundErrors = errors};

            this.verifier.VerifyAll(categories, headers);

            return categories;
        }

        private CategoriesHeaderMap? TryGetHeadersFromFile(List<string>? rowValues)
        {
            if (rowValues == null)
                return null;

            var headerMap = new CategoriesHeaderMap();
            for (int i = 0; i < rowValues.Count; i++)
            {
                var rowValue = rowValues[i];

                switch (rowValue)
                {
                    case "text":     headerMap.TextIndex     = i.ToString(); break;
                    case "id":       headerMap.IdIndex       = i.ToString(); break;
                    case "parentid": headerMap.ParentIdIndex = i.ToString(); break;
                    case "attachmentName": headerMap.AttachmentName = i.ToString(); break;
                    default:
                        return null;
                }
            }

            if (string.IsNullOrEmpty(headerMap.IdIndex) || string.IsNullOrEmpty(headerMap.TextIndex))
                return null;

            return headerMap;
        }

        private CategoriesRow GetRowValues(List<string> row, CategoriesHeaderMap headerMap, int rowNumber) => new CategoriesRow
        {
            Id = GetRowValue(row, headerMap.IdIndex),
            Text = GetRowValue(row, headerMap.TextIndex) ?? String.Empty,
            ParentId = GetRowValue(row, headerMap.ParentIdIndex),
            RowId = rowNumber,
            AttachmentName = GetRowValue(row, headerMap.AttachmentName)
        };

        private string? GetRowValue(List<string> row, string? index)
        {
            return !int.TryParse(index, out var i) ? null : row.ElementAtOrDefault(i);
        }
    }
}
