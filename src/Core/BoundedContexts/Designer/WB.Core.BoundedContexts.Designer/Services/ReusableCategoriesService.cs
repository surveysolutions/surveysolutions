using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Categories;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.SharedKernels.Questionnaire.Categories;

namespace WB.Core.BoundedContexts.Designer.Services
{
    internal class ReusableCategoriesService : IReusableCategoriesService
    {
        private readonly DesignerDbContext dbContext;
        private readonly IQuestionnaireViewFactory questionnaireStorage;
        private readonly ICategoriesExtractFactory categoriesExtractFactory;

        public ReusableCategoriesService(DesignerDbContext dbContext, 
            IQuestionnaireViewFactory questionnaireStorage, 
            ICategoriesExtractFactory categoriesExtractFactory)
        {
            this.dbContext = dbContext;
            this.questionnaireStorage = questionnaireStorage;
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

        public byte[] GetTemplate(CategoriesFileType fileType)
        {
            var extractService = this.categoriesExtractFactory.GetExtractService(fileType);
            return extractService.GetTemplateFile();
        }
        
        public void DeleteAllByQuestionnaireId(Guid questionnaireId)
        {
            var questionnaire = this.questionnaireStorage.Load(new QuestionnaireRevision(questionnaireId));
            if (questionnaire == null)
                return;

            foreach (var categories in questionnaire.Source.Categories)
            {
                this.dbContext.CategoriesInstances.RemoveRange(
                    this.dbContext.CategoriesInstances.Where(x => x.CategoriesId == categories.Id && x.QuestionnaireId == questionnaire.PublicKey));
            }

            this.dbContext.SaveChanges();
        }

        public CategoriesFile? GetAsFile(QuestionnaireRevision questionnaireRevision, Guid categoriesId, CategoriesFileType fileType)
        {
            var questionnaire = this.questionnaireStorage.Load(questionnaireRevision);
            if (questionnaire == null)
                return null;

            var items = this.dbContext.CategoriesInstances
                .Where(x => x.QuestionnaireId == questionnaire.PublicKey
                            && x.CategoriesId == categoriesId)
                .OrderBy(x => x.SortIndex)
                .Select(i => new CategoriesItem()
                {
                    Id = i.Value,
                    ParentId = i.ParentId,
                    Text = i.Text,
                    AttachmentName = i.AttachmentName
                });
            
            var extractService = this.categoriesExtractFactory.GetExtractService(fileType);
            
            return new CategoriesFile
            {
                QuestionnaireTitle = questionnaire.Title,
                CategoriesName = questionnaire.Source.Categories.FirstOrDefault(x => x.Id == categoriesId)?.Name ?? string.Empty,
                Content = extractService.GetAsFile(items.ToList())
            };
        }

        public IQueryable<CategoriesItem> GetCategoriesById(Guid questionnaireId, Guid id) =>
            this.dbContext.CategoriesInstances
                .Where(x => x.QuestionnaireId == questionnaireId && x.CategoriesId == id)
                .OrderBy(x => x.SortIndex)
                .Select(x => new CategoriesItem
                {
                    Id = x.Value,
                    ParentId = x.ParentId,
                    Text = x.Text,
                    AttachmentName = x.AttachmentName
                });

        public void Store(Guid questionnaireId, Guid categoriesId, Stream file, CategoriesFileType fileType)
        {
            //if (categoriesId == null) throw new ArgumentNullException(nameof(categoriesId));
            
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
                Value = int.TryParse(x.Id, out int result) ? result : throw new InvalidOperationException($"Invalid category value {x.Id}"),
                Text = x.Text,
                ParentId = string.IsNullOrEmpty(x.ParentId)
                    ? (int?)null
                    : int.Parse(x.ParentId),
                AttachmentName = x.AttachmentName
            }));
        }
    }
}
