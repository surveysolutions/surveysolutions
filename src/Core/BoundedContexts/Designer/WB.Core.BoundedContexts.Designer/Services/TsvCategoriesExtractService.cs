using System.Collections.Generic;
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
        private ICategoriesVerifier verifier;

        public TsvCategoriesExtractService(ICategoriesVerifier verifier)
        {
            this.verifier = verifier;
        }

        private static Configuration CreateCsvConfiguration() => new Configuration
        {
            HasHeaderRecord = false,
            TrimOptions = TrimOptions.Trim,
            IgnoreQuotes = false,
            Delimiter = "\t"

        };

        public List<CategoriesRow> Extract(Stream file)
        {
            var headers = new CategoriesHeaderMap
            {
                TextIndex = "text",
                IdIndex = "id",
                ParentIdIndex = "parentid"
            };

            var categories = new List<CategoriesRow>();
            var errors = new List<TranslationValidationError>();

            using (var csvReader = new CsvParser(new StreamReader(file), CreateCsvConfiguration()))
            {
                while (true)
                {
                    var rawRow = csvReader.Read()?.ToList();
                    if (rawRow == null) break;

                    var row = GetRowValues(rawRow, csvReader.Context.RawRow);

                    if (string.IsNullOrEmpty(row.Id) && string.IsNullOrEmpty(row.ParentId) &&
                        string.IsNullOrEmpty(row.Text)) continue;

                    var error = this.verifier.Verify(row, headers);

                    if (error != null) errors.Add(error);
                    else categories.Add(row);

                    if (errors.Count >= 10) break;

                    if (categories.Count > AbstractVerifier.MaxOptionsCountInFilteredComboboxQuestion)
                        throw new InvalidExcelFileException(ExceptionMessages.Excel_Categories_More_Than_Limit.FormatString(AbstractVerifier.MaxOptionsCountInFilteredComboboxQuestion));
                }
            }

            if (errors.Any())
                throw new InvalidExcelFileException(ExceptionMessages.TranlationExcelFileHasErrors){FoundErrors = errors};

            this.verifier.VerifyAll(categories, headers);

            return categories;
        }

        private CategoriesRow GetRowValues(List<string> row, int rowNumber) => new CategoriesRow
        {
            Id = row.ElementAtOrDefault(0),
            Text = row.ElementAtOrDefault(1),
            ParentId = row.ElementAtOrDefault(2),
            RowId = rowNumber
        };
    }
}
