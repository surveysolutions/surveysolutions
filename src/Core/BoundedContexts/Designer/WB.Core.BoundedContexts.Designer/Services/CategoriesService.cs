using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Main.Core.Documents;
using OfficeOpenXml;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Categories;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.Verifier;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.SurveySolutions.ReusableCategories;

namespace WB.Core.BoundedContexts.Designer.Services
{
    internal class CategoriesService : ICategoriesService
    {
        private readonly DesignerDbContext dbContext;
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage;
        private readonly ICategoriesExportService categoriesExportService;

        public CategoriesService(DesignerDbContext dbContext, 
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage, 
            ICategoriesExportService categoriesExportService)
        {
            this.dbContext = dbContext;
            this.questionnaireStorage = questionnaireStorage;
            this.categoriesExportService = categoriesExportService;
        }

        public void CloneCategories(Guid questionnaireId, Guid categoriesId, Guid clonedQuestionnaireId, Guid clonedCategoriesId)
        {
            var storedCategoriesList = this.dbContext.CategoriesInstances
                .Where(x => x.QuestionnaireId == questionnaireId && x.CategoriesId == categoriesId)
                .ToList();

            foreach (var storedCategories in storedCategoriesList)
            {
                var categoriesCopy = storedCategories.Clone();
                categoriesCopy.CategoriesId = clonedCategoriesId;
                categoriesCopy.QuestionnaireId = clonedQuestionnaireId;
                this.dbContext.CategoriesInstances.Add(categoriesCopy);
            }
        }

        public byte[] GetTemplateAsExcelFile()
        {
            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                var worksheet = excelPackage.Workbook.Worksheets.Add("Categories");

                worksheet.Cells["A1"].Value = "id";
                worksheet.Cells["B1"].Value = "text";
                worksheet.Cells["C1"].Value = "parentid";

                void FormatCell(string address)
                {
                    var cell = worksheet.Cells[address];
                    cell.Style.Font.Bold = true;
                }

                FormatCell("A1");
                FormatCell("B1");
                FormatCell("C1");

                return excelPackage.GetAsByteArray();
            }
        }

        public CategoriesFile GetAsExcelFile(Guid questionnaireId, Guid categoriesId)
        {
            var questionnaire = this.questionnaireStorage.GetById(questionnaireId.ToString("N"));

            return new CategoriesFile
            {
                QuestionnaireTitle = questionnaire.Title,
                CategoriesName = questionnaire.Categories.FirstOrDefault(x => x.Id == categoriesId)?.Name ?? string.Empty,
                Content = this.GetExcelFileContentEEPlus(questionnaireId, categoriesId)
            };
        }

        public void DeleteAllByQuestionnaireId(Guid questionnaireId)
        {
            var questionnaire = this.questionnaireStorage.GetById(questionnaireId.FormatGuid());
            if (questionnaire == null)
                return;

            foreach (var categories in questionnaire.Categories)
            {
                this.dbContext.CategoriesInstances.RemoveRange(
                    this.dbContext.CategoriesInstances.Where(x => x.CategoriesId == categories.Id && x.QuestionnaireId == questionnaireId));
            }

            this.dbContext.SaveChanges();
        }

        private byte[] GetExcelFileContentEEPlus(Guid questionnaireId, Guid categoriesId)
        {
            var items = this.dbContext.CategoriesInstances
                .Where(x => x.QuestionnaireId == questionnaireId && x.CategoriesId == categoriesId)
                .Select(i => new CategoriesItem()
                {
                    Id = i.Value,
                    ParentId = i.ParentId,
                    Text = i.Text
                })
                .OrderBy(x => x.Id);
            return categoriesExportService.GetAsExcelFile(items);
        }

        public IQueryable<CategoriesItem> GetCategoriesById(Guid questionnaireId, Guid id) =>
            this.dbContext.CategoriesInstances
                .Where(x => x.QuestionnaireId == questionnaireId && x.CategoriesId == id)
                .OrderBy(x => x.SortIndex)
                .Select(x => new CategoriesItem
                {
                    Id = x.Value,
                    ParentId = x.ParentId,
                    Text = x.Text
                });

        public void Store(Guid questionnaireId, Guid categoriesId, byte[] excelRepresentation)
        {
            if (categoriesId == null) throw new ArgumentNullException(nameof(categoriesId));
            if (excelRepresentation == null) throw new ArgumentNullException(nameof(excelRepresentation));

            using (MemoryStream stream = new MemoryStream(excelRepresentation))
            {
                try
                {
                    using (ExcelPackage package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        var headers = GetHeaders(worksheet);

                        if(worksheet.Dimension.End.Row > AbstractVerifier.MaxOptionsCountInFilteredComboboxQuestion + 1)
                            throw new InvalidExcelFileException(ExceptionMessages.Excel_Categories_More_Than_Limit.FormatString(AbstractVerifier.MaxOptionsCountInFilteredComboboxQuestion));

                        var errors = this.Verify(worksheet).Take(10).ToList();
                        if (errors.Any())
                            throw new InvalidExcelFileException(ExceptionMessages.TranlationExcelFileHasErrors) {FoundErrors = errors};

                        if (worksheet.Dimension.End.Row == 1)
                            throw new InvalidExcelFileException(ExceptionMessages.Excel_NoCategories);

                        int sortIndex = 0;
                        var categoriesRows = new SortedList<int, CategoriesRow>();

                        for (int rowNumber = 2; rowNumber <= worksheet.Dimension.End.Row; rowNumber++)
                        {
                            var categories = GetRowValues(worksheet, headers, rowNumber);
                            if(string.IsNullOrEmpty(categories.Id) && string.IsNullOrEmpty(categories.ParentId) && string.IsNullOrEmpty(categories.Text)) continue;

                            categoriesRows.Add(sortIndex++, categories);
                        }

                        ThrowIfNoCategories(categoriesRows.Values);
                        ThrowIfLessThan2Categories(categoriesRows.Values);
                        ThrowIfTextLengthMoreThan250(categoriesRows.Values, headers);
                        ThrowIfParentIdIsEmpty(categoriesRows.Values);
                        ThrowIfDuplicatedByIdAndParentId(categoriesRows.Values, headers);
                        ThrowIfDuplicatedByParentIdAndText(categoriesRows.Values, headers);

                        this.dbContext.CategoriesInstances.AddRange(categoriesRows.Select(x => new CategoriesInstance
                        {
                            SortIndex = x.Key,
                            QuestionnaireId = questionnaireId,
                            CategoriesId = categoriesId,
                            Value = int.Parse(x.Value.Id),
                            Text = x.Value.Text,
                            ParentId = string.IsNullOrEmpty(x.Value.ParentId)
                                ? (int?) null
                                : int.Parse(x.Value.ParentId)
                        }));
                    }
                }
                catch (Exception e) when(e is NullReferenceException || e is InvalidDataException || e is COMException)
                {
                    throw new InvalidExcelFileException(ExceptionMessages.CategoriesCantBeExtracted, e);
                }
            }
        }

        private static void ThrowIfNoCategories(IList<CategoriesRow> categoriesRows)
        {
            if (!categoriesRows.Any())
                throw new InvalidExcelFileException(ExceptionMessages.Excel_NoCategories);
        }

        private static void ThrowIfLessThan2Categories(IList<CategoriesRow> categoriesRows)
        {
            if (categoriesRows.Count < 2)
                throw new InvalidExcelFileException(ExceptionMessages.Excel_Categories_Less_2_Options);
        }

        private static void ThrowIfParentIdIsEmpty(IList<CategoriesRow> categoriesRows)
        {
            var countOfCategoriesWithParentId = categoriesRows.Count(x => !string.IsNullOrEmpty(x.ParentId));
            if (countOfCategoriesWithParentId > 0 && countOfCategoriesWithParentId < categoriesRows.Count)
                throw new InvalidExcelFileException(ExceptionMessages.Excel_Categories_Empty_ParentId);
        }

        private static void ThrowIfDuplicatedByIdAndParentId(IList<CategoriesRow> categoriesRows, CategoriesHeaderMap headers)
        {
            List<TranslationValidationError> errors;
            var duplicatedCategories = categoriesRows.GroupBy(x => new {x.Id, x.ParentId})
                .Where(x => x.Count() > 1);

            if (duplicatedCategories.Any())
            {
                errors = duplicatedCategories.Select(x => new TranslationValidationError
                {
                    Message = ExceptionMessages.Excel_Categories_Duplicated.FormatString(string.Join(",",
                        x.Select(y => y.RowId))),
                    ErrorAddress = $"{headers.IdIndex}{x.FirstOrDefault()?.RowId}"
                }).ToList();

                throw new InvalidExcelFileException(ExceptionMessages.TranlationExcelFileHasErrors) {FoundErrors = errors};
            }
        }

        private static void ThrowIfDuplicatedByParentIdAndText(IList<CategoriesRow> categoriesRows, CategoriesHeaderMap headers)
        {
            List<TranslationValidationError> errors;
            var duplicatedCategories = categoriesRows.GroupBy(x => new {x.ParentId, x.Text})
                .Where(x => x.Count() > 1);

            if (duplicatedCategories.Any())
            {
                errors = duplicatedCategories.Select(x => new TranslationValidationError
                {
                    Message = ExceptionMessages.Excel_Categories_Duplicated.FormatString(string.Join(",",
                        x.Select(y => y.RowId))),
                    ErrorAddress = $"{headers.IdIndex}{x.FirstOrDefault()?.RowId}"
                }).ToList();

                throw new InvalidExcelFileException(ExceptionMessages.TranlationExcelFileHasErrors) {FoundErrors = errors};
            }
        }

        private static void ThrowIfTextLengthMoreThan250(IList<CategoriesRow> categoriesRows, CategoriesHeaderMap headers)
        {
            List<TranslationValidationError> errors;
            var rows = categoriesRows.Where(x => x.Text?.Length > AbstractVerifier.MaxOptionLength);

            if (rows.Any())
            {
                errors = rows.Select(x => new TranslationValidationError
                {
                    Message = ExceptionMessages.Excel_Categories_Text_More_Than_250.FormatString($"{headers.TextIndex}{x.RowId}"),
                    ErrorAddress = $"{headers.TextIndex}{x.RowId}"
                }).ToList();

                throw new InvalidExcelFileException(ExceptionMessages.TranlationExcelFileHasErrors) {FoundErrors = errors};
            }
        }

        private class CategoriesHeaderMap
        {
            public string IdIndex { get; set; }
            public string ParentIdIndex { get; set; }
            public string TextIndex { get; set; }
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

        private class CategoriesRow
        {
            public string Id { get; set; }
            public string Text { get; set; }
            public string ParentId { get; set; }

            public int RowId { get; set; }
        }

        private CategoriesRow GetRowValues(ExcelWorksheet worksheet, CategoriesHeaderMap headers, int rowNumber) => new CategoriesRow
        {
            Id = worksheet.Cells[$"{headers.IdIndex}{rowNumber}"].GetValue<string>(),
            Text = worksheet.Cells[$"{headers.TextIndex}{rowNumber}"].GetValue<string>(),
            ParentId = worksheet.Cells[$"{headers.ParentIdIndex}{rowNumber}"].GetValue<string>(),
            RowId = rowNumber
        };

        private IEnumerable<TranslationValidationError> Verify(ExcelWorksheet worksheet)
        {
            var headers = GetHeaders(worksheet);

            if (headers.IdIndex == null)
                yield return new TranslationValidationError
                {
                    Message = string.Format(ExceptionMessages.RequiredHeaderWasNotFound, "id"),
                };
            
            if (headers.TextIndex == null)
                yield return new TranslationValidationError
                {
                    Message = string.Format(ExceptionMessages.RequiredHeaderWasNotFound, "text"),
                };
            
            for (int rowNumber = 2; rowNumber <= worksheet.Dimension.End.Row; rowNumber++)
            {
                var categories = GetRowValues(worksheet, headers, rowNumber);

                if(string.IsNullOrEmpty(categories.Id) && string.IsNullOrEmpty(categories.ParentId) && string.IsNullOrEmpty(categories.Text)) continue;

                var idAddress = $"{headers.IdIndex}{rowNumber}";
                var parentIdAddress = $"{headers.ParentIdIndex}{rowNumber}";
                var textAddress = $"{headers.TextIndex}{rowNumber}";

                if (string.IsNullOrEmpty(categories.Id))
                    yield return new TranslationValidationError
                    {
                        Message = string.Format(ExceptionMessages.Excel_Categories_Empty_Value, idAddress),
                        ErrorAddress = idAddress
                    };

                if (!string.IsNullOrEmpty(categories.Id) && !int.TryParse(categories.Id, out _))
                    yield return new TranslationValidationError
                    {
                        Message = string.Format(ExceptionMessages.Excel_Categories_Int_Invalid, idAddress),
                        ErrorAddress = idAddress
                    };

                if (!string.IsNullOrEmpty(categories.ParentId) && !int.TryParse(categories.ParentId, out _))
                    yield return new TranslationValidationError
                    {
                        Message = string.Format(ExceptionMessages.Excel_Categories_Int_Invalid, parentIdAddress),
                        ErrorAddress = parentIdAddress
                    };

                if (string.IsNullOrEmpty(categories.Text))
                    yield return new TranslationValidationError
                    {
                        Message = string.Format(ExceptionMessages.Excel_Categories_Empty_Text, textAddress),
                        ErrorAddress = textAddress
                    };
            }
        }
    }
}
