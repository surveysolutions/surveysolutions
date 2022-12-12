using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    public class CategoricalOptionsImportService : ICategoricalOptionsImportService
    {
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader;
        private readonly ICategoriesExtractFactory categoriesExtractFactory;

        public CategoricalOptionsImportService(
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader,
            ICategoriesExtractFactory categoriesExtractFactory)
        {
            this.questionnaireDocumentReader = questionnaireDocumentReader;
            this.categoriesExtractFactory = categoriesExtractFactory;
        }

        public ImportCategoricalOptionsResult ImportOptions(Stream file, string questionnaireId, Guid categoricalQuestionId, CategoriesFileType fileType)
        {
            var document = this.questionnaireDocumentReader.GetById(questionnaireId);
            if (document == null)
                return ImportCategoricalOptionsResult.Failed(string.Format(ExceptionMessages.QuestionnaireCantBeFound, questionnaireId));
            
            var question = document?.Find<ICategoricalQuestion>(categoricalQuestionId);
            if (question == null)
                return ImportCategoricalOptionsResult.Failed(string.Format(ExceptionMessages.QuestionCannotBeFound, categoricalQuestionId));

            var extractService = categoriesExtractFactory.GetExtractService(fileType);

            try
            {
                var categoriesRows = extractService.Extract(file);
                return ImportCategoricalOptionsResult.Success(categoriesRows.ToArray());
            }
            catch (InvalidFileException e)
            {
                if (e.FoundErrors != null && e.FoundErrors.Any())
                    return ImportCategoricalOptionsResult.Failed(e.FoundErrors.Select(x => x.Message).ToArray());
                return ImportCategoricalOptionsResult.Failed(e.Message);
            }
            catch (Exception e) when (e is NullReferenceException || e is InvalidDataException || e is COMException)
            {
                throw new InvalidFileException(ExceptionMessages.CategoriesCantBeExtracted, e);
            }
        }

        public Stream ExportOptions(string questionnaireId, Guid categoricalQuestionId, CategoriesFileType fileType)
        {
            var document = this.questionnaireDocumentReader.GetById(questionnaireId);
            var question = document?.Find<IQuestion>(categoricalQuestionId);
            if (question == null)
                throw new InvalidOperationException(string.Format(ExceptionMessages.QuestionCannotBeFound, categoricalQuestionId));

            var options = question.Answers?.Select(option => new CategoriesItem()
            {
                Id = (int) option.GetParsedValue(),
                ParentId = option.GetParsedParentValue(),
                Text = option.AnswerText,
                AttachmentName = option.AttachmentName
            }) ?? Enumerable.Empty<CategoriesItem>();

            var extractService = categoriesExtractFactory.GetExtractService(fileType);
            var bytes = extractService.GetAsFile(options.ToList());

            return new MemoryStream(bytes);
        }
    }
}
