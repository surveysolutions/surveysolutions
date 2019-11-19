using System;
using System.Linq;
using Main.Core.Documents;
using OfficeOpenXml;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
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
                ContentAsExcelFile = this.GetExcelFileContentEEPlus(questionnaireId, categoriesId)
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

        public void Store(Guid questionnaireId, Guid categoriesId, byte[] fileBytes)
        {
            
        }
    }
}
