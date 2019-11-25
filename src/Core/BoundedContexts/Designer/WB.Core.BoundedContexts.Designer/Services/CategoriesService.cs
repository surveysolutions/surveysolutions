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

                        var errors = this.Verify(worksheet).Take(10).ToList();
                        if (errors.Any())
                            throw new InvalidExcelFileException(ExceptionMessages.TranlationExcelFileHasErrors) {FoundErrors = errors};

                        for (int rowNumber = 2; rowNumber <= worksheet.Dimension.End.Row; rowNumber++)
                        {
                            this.dbContext.CategoriesInstances.Add(new CategoriesInstance
                            {
                                QuestionnaireId = questionnaireId,
                                CategoriesId = categoriesId,
                                Id = worksheet.Cells[$"A{rowNumber}"].GetValue<int>(),
                                ParentId = worksheet.Cells[$"B{rowNumber}"].GetValue<int?>(),
                                Text = worksheet.Cells[$"C{rowNumber}"].GetValue<string>()
                            });
                        }

                        this.dbContext.SaveChanges();
                    }
                }
                catch (Exception e) when(e is NullReferenceException || e is InvalidDataException || e is COMException)
                {
                    throw new InvalidExcelFileException(ExceptionMessages.CategoriesCantBeExtracted, e);
                }
            }
        }

        private IEnumerable<TranslationValidationError> Verify(ExcelWorksheet worksheet)
        {
            yield break;
        }
    }
}
