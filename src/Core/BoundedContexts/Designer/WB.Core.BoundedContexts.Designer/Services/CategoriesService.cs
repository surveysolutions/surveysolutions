using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using ClosedXML.Excel;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Categories;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Translations;
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
        private readonly ICategoriesExtractFactory categoriesExtractFactory;

        public CategoriesService(DesignerDbContext dbContext, 
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage, 
            ICategoriesExportService categoriesExportService,
            ICategoriesExtractFactory categoriesExtractFactory)
        {
            this.dbContext = dbContext;
            this.questionnaireStorage = questionnaireStorage;
            this.categoriesExportService = categoriesExportService;
            this.categoriesExtractFactory = categoriesExtractFactory;
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
            using (XLWorkbook excelPackage = new XLWorkbook())
            {
                var worksheet = excelPackage.Worksheets.Add("Categories");

                worksheet.Cells("A1").Value = "id";
                worksheet.Cells("B1").Value = "text";
                worksheet.Cells("C1").Value = "parentid";

                void FormatCell(string address)
                {
                    var cell = worksheet.Cells(address);
                    cell.Style.Font.Bold = true;
                }

                FormatCell("A1");
                FormatCell("B1");
                FormatCell("C1");

                using var stream = new MemoryStream();
                excelPackage.SaveAs(stream);
                return stream.ToArray();
            }
        }

        public CategoriesFile? GetAsExcelFile(Guid questionnaireId, Guid categoriesId)
        {
            var questionnaire = this.questionnaireStorage.GetById(questionnaireId.ToString("N"));
            if (questionnaire == null)
                return null;

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

        public void Store(Guid questionnaireId, Guid categoriesId, Stream file, CategoriesFileType fileType)
        {
            if (categoriesId == null) throw new ArgumentNullException(nameof(categoriesId));
            
            try
            {
                var categoriesRows = GetRowsFromFile(file, fileType);
                this.Store(questionnaireId, categoriesId, categoriesRows);

            }
            catch (Exception e) when (e is NullReferenceException || e is InvalidDataException || e is COMException)
            {
                throw new InvalidFileException(ExceptionMessages.CategoriesCantBeExtracted, e);
            }
        }

        public List<CategoriesRow> GetRowsFromFile(Stream file, CategoriesFileType fileType)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            var extractService = this.categoriesExtractFactory.GetExtractService(fileType);

            try
            {
                return extractService.Extract(file);
            }
            catch (Exception e) when (e is NullReferenceException || e is InvalidDataException || e is COMException)
            {
                throw new InvalidFileException(ExceptionMessages.CategoriesCantBeExtracted, e);
            }
        }

        public void Store(Guid questionnaireId, Guid categoriesId, List<CategoriesRow> categoriesRows)
        {
            this.dbContext.CategoriesInstances.AddRange(categoriesRows.Select((x, i) => new CategoriesInstance
            {
                SortIndex = i,
                QuestionnaireId = questionnaireId,
                CategoriesId = categoriesId,
                Value = int.Parse(x.Id),
                Text = x.Text,
                ParentId = string.IsNullOrEmpty(x.ParentId)
                    ? (int?)null
                    : int.Parse(x.ParentId)
            }));
        }
    }
}
