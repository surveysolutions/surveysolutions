using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.Verifier;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Questionnaire.Categories;

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
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
            Mode = CsvMode.RFC4180,
            Delimiter = "\t"
        };

        public List<CategoriesRow> Extract(Stream file)
        {
            var categories = new List<CategoriesRow>();
            var errors = new List<ImportValidationError>();

            var csvConfiguration = CreateCsvConfiguration();
            csvConfiguration.HasHeaderRecord = false;
            
            using (var csvReader = new CsvParser(new StreamReader(file), csvConfiguration))
            {
                var read = csvReader.Read();
                var rawRow = csvReader.Record;
                var headers = TryGetHeadersFromFile(rawRow);
                if (headers == null)
                {
                    throw new InvalidFileException(ExceptionMessages.ProvidedFileHasErrors)
                    {
                        FoundErrors = new List<ImportValidationError>(new[]
                        {
                            new ImportValidationError
                            {
                                Message = ExceptionMessages.HeaderWasNotFound,
                            }
                        })
                    };
                }

                while (csvReader.Read())
                {
                    rawRow = csvReader.Record;
                    
                    if (rawRow == null) break;

                    var row = GetRowValues(rawRow, headers, csvReader.RawRow);

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
                }

                if (errors.Any())
                    throw new InvalidFileException(ExceptionMessages.ProvidedFileHasErrors) {FoundErrors = errors};

                this.verifier.VerifyAll(categories, headers);
            }


            return categories;
        }

        public class CategoriesItemMap: ClassMap<CategoriesItem>
        {
            protected CategoriesItemMap()
            {
                Map(m => m.Id).Index(0).Name(CategoriesConstants.ValueColumnName);
                Map(m => m.Text).Index(1).Name(CategoriesConstants.TitleColumnName);
                Map(m => m.AttachmentName).Index(2).Name(CategoriesConstants.AttachmentNameColumnName);
            }
        }
        
        private class CascadingItemMap : CategoriesItemMap
        {
            public CascadingItemMap()
            {
                Map(m => m.ParentId).Index(2).Name(CategoriesConstants.ParentValueColumnName);
                Map(m => m.AttachmentName).Index(3); // change index for cascading
            }
        }

        public byte[] GetTemplateFile(bool isCascading)
        {
            return GetCsvFile(isCascading, new List<CategoriesItem>());
        }

        private static byte[] GetCsvFile(bool isCascading, List<CategoriesItem> options)
        {
            var cfg = CreateCsvConfiguration();

            var sb = new StringBuilder();
            using (var csvWriter = new CsvWriter(new StringWriter(sb), cfg))
            {
                if (isCascading)
                    csvWriter.Context.RegisterClassMap<CascadingItemMap>();
                else
                    csvWriter.Context.RegisterClassMap<CategoriesItemMap>();
                
                csvWriter.WriteRecords(options);
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return bytes;
        }

        public byte[] GetAsFile(List<CategoriesItem> items, bool isCascading, bool hqImport)
        {
            return GetCsvFile(isCascading, items);
        }

        private CategoriesHeaderMap? TryGetHeadersFromFile(string[]? rowValues)
        {
            if (rowValues == null)
                return null;

            var headerMap = new CategoriesHeaderMap();
            for (int i = 0; i < rowValues.Length; i++)
            {
                var rowValue = rowValues[i];

                switch (rowValue)
                {
                    case CategoriesConstants.TitleColumnName:       
                    case CategoriesConstants.OldTitleColumnName:       
                        headerMap.TextIndex = i.ToString(); break;
                    case CategoriesConstants.ValueColumnName:       
                    case CategoriesConstants.OldValueColumnName:       
                        headerMap.IdIndex = i.ToString(); break;
                    case CategoriesConstants.ParentValueColumnName: 
                    case CategoriesConstants.OldParentValueColumnName: 
                        headerMap.ParentIdIndex = i.ToString(); break;
                    case CategoriesConstants.AttachmentNameColumnName: 
                        headerMap.AttachmentNameIndex = i.ToString(); break;
                    default:
                        return null;
                }
            }

            if (string.IsNullOrEmpty(headerMap.IdIndex) || string.IsNullOrEmpty(headerMap.TextIndex))
                return null;

            return headerMap;
        }

        private CategoriesRow GetRowValues(string[]? row, CategoriesHeaderMap headerMap, int rowNumber) => new CategoriesRow
        {
            Id = GetRowValue(row, headerMap.IdIndex),
            Text = GetRowValue(row, headerMap.TextIndex) ?? String.Empty,
            ParentId = GetRowValue(row, headerMap.ParentIdIndex),
            RowId = rowNumber,
            AttachmentName = GetRowValue(row, headerMap.AttachmentNameIndex)
        };

        private string? GetRowValue(string[]? row, string? index)
        {
            if (row == null) return null;
            
            return !int.TryParse(index, out var i) ? null : row.ElementAtOrDefault(i);
        }
    }
}
