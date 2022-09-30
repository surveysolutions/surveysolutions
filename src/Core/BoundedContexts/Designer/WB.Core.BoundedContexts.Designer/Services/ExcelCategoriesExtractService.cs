﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.Verifier;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Core.BoundedContexts.Designer.Services
{
    internal class ExcelCategoriesExtractService : ICategoriesExtractService
    {
        private readonly ICategoriesVerifier verifier;

        public ExcelCategoriesExtractService(ICategoriesVerifier verifier)
        {
            this.verifier = verifier;
        }

        public List<CategoriesRow> Extract(Stream file)
        {
            return file.WrapStreamIntoTempFile(excel => ExtractCategoriesFromExcelFile(excel));
        }

        private List<CategoriesRow> ExtractCategoriesFromExcelFile(Stream xmlFile)
        {
            var categories = new List<CategoriesRow>();
            using XLWorkbook package = new XLWorkbook(xmlFile);
            var worksheet = package.Worksheets.First();
            var headers = GetHeaders(worksheet);

            var rowsCount = worksheet.LastRowUsed().RowNumber();
            if (rowsCount > AbstractVerifier.MaxOptionsCountInFilteredComboboxQuestion + 1)
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

            if (rowsCount == 1)
                throw new InvalidFileException(ExceptionMessages.Excel_NoCategories);

            var errors = new List<ImportValidationError>();
            for (int rowNumber = 2; rowNumber <= rowsCount; rowNumber++)
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

        private CategoriesHeaderMap GetHeaders(IXLWorksheet worksheet)
        {
            var headers = new List<Tuple<string, string>>()
            {
                new Tuple<string, string>(worksheet.Cell("A1").GetString(), "A"),
                new Tuple<string, string>(worksheet.Cell("B1").GetString(), "B"),
                new Tuple<string, string>(worksheet.Cell("C1").GetString(), "C"),
                new Tuple<string, string>(worksheet.Cell("D1").GetString(), "D")
            }.Where(kv => !string.IsNullOrEmpty(kv.Item1))
                .ToDictionary(k => k.Item1.Trim(), v => v.Item2);

            return new CategoriesHeaderMap()
            {
                IdIndex = headers.GetOrNull("id"),
                ParentIdIndex = headers.GetOrNull("parentid"),
                TextIndex = headers.GetOrNull("text"),
                AttachmentNameIndex = headers.GetOrNull("attachmentname"),
            };
        }

        private CategoriesRow GetRowValues(IXLWorksheet worksheet, CategoriesHeaderMap headers, int rowNumber) => new CategoriesRow
        {
            Id = worksheet.Cell($"{headers.IdIndex}{rowNumber}").GetString(),
            Text = worksheet.Cell($"{headers.TextIndex}{rowNumber}").GetString(),
            ParentId = worksheet.Cell($"{headers.ParentIdIndex}{rowNumber}").GetString(),
            RowId = rowNumber,
            AttachmentName = headers.AttachmentNameIndex != null ? worksheet.Cell($"{headers.AttachmentNameIndex}{rowNumber}").GetString() : null,
        };
    }
}
