﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.Verifier;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Designer.Services
{
    internal class ExcelCategoriesExtractService : ICategoriesExtractService
    {
        private ICategoriesVerifier verifier;

        public ExcelCategoriesExtractService(ICategoriesVerifier verifier)
        {
            this.verifier = verifier;
        }

        public List<CategoriesRow> Extract(Stream file)
        {
            var categories = new List<CategoriesRow>();

            using (ExcelPackage package = new ExcelPackage(file))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var headers = GetHeaders(worksheet);

                if (worksheet.Dimension.End.Row > AbstractVerifier.MaxOptionsCountInFilteredComboboxQuestion + 1)
                    throw new InvalidFileException(
                        ExceptionMessages.Excel_Categories_More_Than_Limit.FormatString(AbstractVerifier
                            .MaxOptionsCountInFilteredComboboxQuestion));

                if (headers.IdIndex == null)
                    throw new InvalidFileException(ExceptionMessages.ProvidedFileHasErrors)
                    {
                        FoundErrors = new List<ImportValidationError>(new[]
                        {
                            new ImportValidationError
                            {
                                Message = string.Format(ExceptionMessages.RequiredHeaderWasNotFound, "id")
                            }
                        })
                    };

                if (headers.TextIndex == null)
                    throw new InvalidFileException(ExceptionMessages.ProvidedFileHasErrors)
                    {
                        FoundErrors = new List<ImportValidationError>(new[]
                        {
                            new ImportValidationError
                            {
                                Message = string.Format(ExceptionMessages.RequiredHeaderWasNotFound, "text")
                            }
                        })
                    };

                if (worksheet.Dimension.End.Row == 1)
                    throw new InvalidFileException(ExceptionMessages.Excel_NoCategories);

                var errors = new List<ImportValidationError>();
                for (int rowNumber = 2; rowNumber <= worksheet.Dimension.End.Row; rowNumber++)
                {
                    var row = GetRowValues(worksheet, headers, rowNumber);

                    if (string.IsNullOrEmpty(row.Id) && string.IsNullOrEmpty(row.ParentId) &&
                        string.IsNullOrEmpty(row.Text)) continue;

                    var error = this.verifier.Verify(row, headers);

                    if (error != null) errors.Add(error);
                    else categories.Add(row);

                    if (errors.Count >= 10) break;
                }

                if (errors.Any())
                    throw new InvalidFileException(ExceptionMessages.ProvidedFileHasErrors) {FoundErrors = errors};

                this.verifier.VerifyAll(categories, headers);

                return categories;
            }
        }

        private CategoriesHeaderMap GetHeaders(ExcelWorksheet worksheet)
        {
            var headers = new List<Tuple<string, string>>()
            {
                new Tuple<string, string>(worksheet.Cells["A1"].GetValue<string>(), "A"),
                new Tuple<string, string>(worksheet.Cells["B1"].GetValue<string>(), "B"),
                new Tuple<string, string>(worksheet.Cells["C1"].GetValue<string>(), "C")
            }.Where(kv => kv.Item1 != null).ToDictionary(k => k.Item1.Trim(), v => v.Item2);

            return new CategoriesHeaderMap()
            {
                IdIndex = headers.GetOrNull("id"),
                ParentIdIndex = headers.GetOrNull("parentid"),
                TextIndex = headers.GetOrNull("text"),
            };
        }

        private CategoriesRow GetRowValues(ExcelWorksheet worksheet, CategoriesHeaderMap headers, int rowNumber) => new CategoriesRow
        {
            Id = worksheet.Cells[$"{headers.IdIndex}{rowNumber}"].GetValue<string>(),
            Text = worksheet.Cells[$"{headers.TextIndex}{rowNumber}"].GetValue<string>(),
            ParentId = worksheet.Cells[$"{headers.ParentIdIndex}{rowNumber}"].GetValue<string>(),
            RowId = rowNumber
        };
    }
}
