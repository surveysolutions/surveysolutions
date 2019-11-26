using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using CsvHelper;
using CsvHelper.Configuration;
using Main.Core.Documents;
using OfficeOpenXml;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Categories;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Categories;

namespace WB.Core.BoundedContexts.Designer.Services
{
    internal class CategoriesService : ICategoriesService
    {
        private readonly DesignerDbContext dbContext;
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage;

        public CategoriesService(DesignerDbContext dbContext, IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage)
        {
            this.dbContext = dbContext;
            this.questionnaireStorage = questionnaireStorage;
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
                worksheet.Cells["B1"].Value = "parentid";
                worksheet.Cells["C1"].Value = "text";

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
                ContentFile = this.GetExcelFileContentEEPlus(questionnaireId, categoriesId)
            };
        }

        public IQueryable<CategoriesItem> GetCategoriesById(Guid id) =>
            this.dbContext.CategoriesInstances.Where(x => x.CategoriesId == id).Select(x => new CategoriesItem
            {
                Id = x.Id,
                ParentId = x.ParentId,
                Text = x.Text
            });

        private byte[] GetExcelFileContentEEPlus(Guid questionnaireId, Guid categoriesId)
        {
            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                var worksheet = excelPackage.Workbook.Worksheets.Add("Categories");

                worksheet.Cells["A1"].Value = "id";
                worksheet.Cells["B1"].Value = "parentid";
                worksheet.Cells["C1"].Value = "text";

                void FormatCell(string address)
                {
                    var cell = worksheet.Cells[address];
                    cell.Style.Font.Bold = true;
                }

                FormatCell("A1");
                FormatCell("B1");
                FormatCell("C1");

                int currentRowNumber = 1;

                foreach (var row in this.dbContext.CategoriesInstances.Where(x =>
                    x.QuestionnaireId == questionnaireId && x.CategoriesId == categoriesId))
                {
                    currentRowNumber++;

                    worksheet.Cells[$"A{currentRowNumber}"].Value = row.Id;
                    worksheet.Cells[$"A{currentRowNumber}"].Style.WrapText = true;
                    worksheet.Cells[$"B{currentRowNumber}"].Value = row.ParentId;
                    worksheet.Cells[$"B{currentRowNumber}"].Style.WrapText = true;
                    worksheet.Cells[$"C{currentRowNumber}"].Value = row.Text;
                    worksheet.Cells[$"C{currentRowNumber}"].Style.WrapText = true;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet.Column(3).AutoFit();
                worksheet.Protection.AllowFormatColumns = true;

                return excelPackage.GetAsByteArray();
            }
        }

        public CategoriesFile GetPlainContentFile(Guid questionnaireId, Guid categoriesId)
        {
            var questionnaire = this.questionnaireStorage.GetById(questionnaireId.ToString("N"));

            return new CategoriesFile
            {
                QuestionnaireTitle = questionnaire.Title,
                CategoriesName = questionnaire.Categories.FirstOrDefault(x => x.Id == categoriesId)?.Name ?? string.Empty,
                ContentFile = this.GetTabFileContent(questionnaireId, categoriesId)
            };
        }

        private byte[] GetTabFileContent(Guid questionnaireId, Guid categoriesId)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var csvWriter = new CsvWriter(new StreamWriter(memoryStream), this.CreateCsvConfiguration()))
                {
                    csvWriter.WriteField("id");
                    csvWriter.WriteField("parentid");
                    csvWriter.WriteField("text");
                    csvWriter.NextRecord();

                    foreach (var row in this.dbContext.CategoriesInstances.Where(x => x.QuestionnaireId == questionnaireId && x.CategoriesId == categoriesId))
                    {
                        csvWriter.WriteField(row.Id);
                        csvWriter.WriteField(row.ParentId);
                        csvWriter.WriteField(row.Text);
                        csvWriter.NextRecord();
                    }
                }

                return memoryStream.ToArray();
            }
        }

        private Configuration CreateCsvConfiguration()
        {
            return new Configuration { HasHeaderRecord = true, TrimOptions = TrimOptions.Trim, IgnoreQuotes = false, Delimiter = "\t", MissingFieldFound = null };
        }

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
                        if (package.Workbook.Worksheets.Count == 0)
                        {
                            throw new InvalidExcelFileException(ExceptionMessages.TranslationFileIsEmpty);
                        }

                        var worksheet = package.Workbook.Worksheets[0];
                        var headers = GetHeaders(worksheet);

                        var errors = this.Verify(worksheet).Take(10).ToList();
                        if (errors.Any())
                            throw new InvalidExcelFileException(ExceptionMessages.TranlationExcelFileHasErrors) {FoundErrors = errors};

                        for (int rowNumber = 2; rowNumber <= worksheet.Dimension.End.Row; rowNumber++)
                        {
                            var categories = GetRowValues(worksheet, headers, rowNumber);
                            if(string.IsNullOrEmpty(categories.Id) && string.IsNullOrEmpty(categories.ParentId) && string.IsNullOrEmpty(categories.Text)) continue;

                            this.dbContext.CategoriesInstances.Add(new CategoriesInstance
                            {
                                QuestionnaireId = questionnaireId,
                                CategoriesId = categoriesId,
                                Id = int.Parse(categories.Id),
                                ParentId = string.IsNullOrEmpty(categories.ParentId) ? (int?)null : int.Parse(categories.ParentId),
                                Text = categories.Text
                            });
                        }
                    }
                }
                catch (Exception e) when(e is NullReferenceException || e is InvalidDataException || e is COMException)
                {
                    throw new InvalidExcelFileException(ExceptionMessages.CategoriesCantBeExtracted, e);
                }
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
        }

        private CategoriesRow GetRowValues(ExcelWorksheet worksheet, CategoriesHeaderMap headers, int rowNumber) => new CategoriesRow
        {
            Id = worksheet.Cells[$"{headers.IdIndex}{rowNumber}"].GetValue<string>(),
            Text = worksheet.Cells[$"{headers.TextIndex}{rowNumber}"].GetValue<string>(),
            ParentId = worksheet.Cells[$"{headers.ParentIdIndex}{rowNumber}"].GetValue<string>()
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

            if (headers.IdIndex == null || headers.TextIndex == null)
                yield break;

            for (int rowNumber = 2; rowNumber <= worksheet.Dimension.End.Row; rowNumber++)
            {
                var categories = GetRowValues(worksheet, headers, rowNumber);

                if(string.IsNullOrEmpty(categories.Id) && string.IsNullOrEmpty(categories.ParentId) && string.IsNullOrEmpty(categories.Text)) continue;

                var cellAddress = $"{headers.IdIndex}{rowNumber}";

                if (string.IsNullOrEmpty(categories.Id))
                    yield return new TranslationValidationError
                    {
                        Message = string.Format(ExceptionMessages.Excel_Categories_Empty_Value, cellAddress),
                        ErrorAddress = cellAddress
                    };

                if (!string.IsNullOrEmpty(categories.Id) && !int.TryParse(categories.Id, out _))
                    yield return new TranslationValidationError
                    {
                        Message = string.Format(ExceptionMessages.Excel_Categories_Int_Invalid, cellAddress),
                        ErrorAddress = cellAddress
                    };

                if (!string.IsNullOrEmpty(categories.ParentId) && !int.TryParse(categories.ParentId, out _))
                    yield return new TranslationValidationError
                    {
                        Message = string.Format(ExceptionMessages.Excel_Categories_Int_Invalid, cellAddress),
                        ErrorAddress = cellAddress
                    };

                if (string.IsNullOrEmpty(categories.Text))
                    yield return new TranslationValidationError
                    {
                        Message = string.Format(ExceptionMessages.Excel_Categories_Empty_Text, cellAddress),
                        ErrorAddress = cellAddress
                    };
            }
        }
    }
}
