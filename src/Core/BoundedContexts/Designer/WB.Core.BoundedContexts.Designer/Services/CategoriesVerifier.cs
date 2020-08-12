using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.Verifier;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Designer.Services
{
    internal class CategoriesVerifier : ICategoriesVerifier
    {
        public ImportValidationError? Verify(CategoriesRow categories, CategoriesHeaderMap headers)
        {
            if (categories == null) throw new ArgumentNullException(nameof(categories));

            if(string.IsNullOrEmpty(categories.Id) && string.IsNullOrEmpty(categories.ParentId) && string.IsNullOrEmpty(categories.Text))
                throw new ArgumentException("Categories should not be empty", nameof(categories));

            var messageFormat = $"{ExceptionMessages.Column}: {{0}}, {ExceptionMessages.Row}: {categories.RowId}";

            var idAddress = string.Format(messageFormat, headers.IdIndex);
            var parentIdAddress = string.Format(messageFormat, headers.ParentIdIndex);
            var textAddress = string.Format(messageFormat, headers.TextIndex);

            if (string.IsNullOrEmpty(categories.Id))
                return new ImportValidationError
                {
                    Message = string.Format(ExceptionMessages.Excel_Categories_Empty_Value, idAddress),
                    ErrorAddress = idAddress
                };

            if (!string.IsNullOrEmpty(categories.Id) && !int.TryParse(categories.Id, out _))
                return new ImportValidationError
                {
                    Message = string.Format(ExceptionMessages.Excel_Categories_Int_Invalid, idAddress),
                    ErrorAddress = idAddress
                };

            if (!string.IsNullOrEmpty(categories.ParentId) && !int.TryParse(categories.ParentId, out _))
                return new ImportValidationError
                {
                    Message = string.Format(ExceptionMessages.Excel_Categories_Int_Invalid, parentIdAddress),
                    ErrorAddress = parentIdAddress
                };

            if (string.IsNullOrEmpty(categories.Text))
                return new ImportValidationError
                {
                    Message = string.Format(ExceptionMessages.Excel_Categories_Empty_Text, textAddress),
                    ErrorAddress = textAddress
                };

            return null;
        }

        public void VerifyAll(List<CategoriesRow> rows, CategoriesHeaderMap headers)
        {
            ThrowIfNoCategories(rows);
            ThrowIfLessThan2Categories(rows);
            ThrowIfTextLengthMoreThan250(rows, headers);
            ThrowIfParentIdIsEmpty(rows);
            ThrowIfDuplicatedByIdAndParentId(rows, headers);
            ThrowIfDuplicatedByParentIdAndText(rows, headers);
        }


        private static void ThrowIfNoCategories(IList<CategoriesRow> categoriesRows)
        {
            if (!categoriesRows.Any())
                throw new InvalidFileException(ExceptionMessages.Excel_NoCategories);
        }

        private static void ThrowIfLessThan2Categories(IList<CategoriesRow> categoriesRows)
        {
            if (categoriesRows.Count < 2)
                throw new InvalidFileException(ExceptionMessages.Excel_Categories_Less_2_Options);
        }

        private static void ThrowIfParentIdIsEmpty(IList<CategoriesRow> categoriesRows)
        {
            var countOfCategoriesWithParentId = categoriesRows.Count(x => !string.IsNullOrEmpty(x.ParentId));
            if (countOfCategoriesWithParentId > 0 && countOfCategoriesWithParentId < categoriesRows.Count)
                throw new InvalidFileException(ExceptionMessages.Excel_Categories_Empty_ParentId);
        }

        private static void ThrowIfDuplicatedByIdAndParentId(IList<CategoriesRow> categoriesRows, CategoriesHeaderMap headers)
        {
            List<ImportValidationError> errors;
            var duplicatedCategories = categoriesRows.GroupBy(x => new {x.Id, x.ParentId})
                .Where(x => x.Count() > 1);

            if (duplicatedCategories.Any())
            {
                errors = duplicatedCategories.Select(x => new ImportValidationError
                {
                    Message = ExceptionMessages.Excel_Categories_Duplicated.FormatString(string.Join(",",
                        x.Select(y => y.RowId))),
                    ErrorAddress = $"{headers.IdIndex}{x.FirstOrDefault()?.RowId}"
                }).ToList();

                throw new InvalidFileException(ExceptionMessages.ProvidedFileHasErrors) {FoundErrors = errors};
            }
        }

        private static void ThrowIfDuplicatedByParentIdAndText(IList<CategoriesRow> categoriesRows, CategoriesHeaderMap headers)
        {
            List<ImportValidationError> errors;
            var duplicatedCategories = categoriesRows.GroupBy(x => new {x.ParentId, x.Text})
                .Where(x => x.Count() > 1);

            if (duplicatedCategories.Any())
            {
                errors = duplicatedCategories.Select(x => new ImportValidationError
                {
                    Message = ExceptionMessages.Excel_Categories_Duplicated.FormatString(string.Join(",",
                        x.Select(y => y.RowId))),
                    ErrorAddress = $"{headers.IdIndex}{x.FirstOrDefault()?.RowId}"
                }).ToList();

                throw new InvalidFileException(ExceptionMessages.ProvidedFileHasErrors) {FoundErrors = errors};
            }
        }

        private static void ThrowIfTextLengthMoreThan250(IList<CategoriesRow> categoriesRows, CategoriesHeaderMap headers)
        {
            List<ImportValidationError> errors;
            var rows = categoriesRows.Where(x => x.Text?.Length > AbstractVerifier.MaxOptionLength);

            if (rows.Any())
            {
                errors = rows.Select(x => new ImportValidationError
                {
                    Message = ExceptionMessages.Excel_Categories_Text_More_Than_250.FormatString($"{headers.TextIndex}{x.RowId}"),
                    ErrorAddress = $"{headers.TextIndex}{x.RowId}"
                }).ToList();

                throw new InvalidFileException(ExceptionMessages.ProvidedFileHasErrors) {FoundErrors = errors};
            }
        }
    }
}
