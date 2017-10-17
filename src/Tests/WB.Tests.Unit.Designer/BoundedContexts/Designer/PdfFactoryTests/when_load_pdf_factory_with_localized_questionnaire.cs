using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.PdfFactoryTests
{
    public class when_load_pdf_factory_with_localized_questionnaire : PdfFactoryTestsContext
    {
        private Mock<IQuestionnaireTranslator> translator;
        private Mock<ITranslationsService> translationsService;

        [OneTimeSetUp]
        public void Context()
        {
            var accountDocument = Create.AccountDocument(userName);
            
            this.questionnaireDocument = Create.QuestionnaireDocument(questionnaireId, title: "Na Movi");
            var translatedDocument = Create.QuestionnaireDocument(questionnaireId, title: "На мови");

            var accountsDocumentReader = Mock.Of<IPlainStorageAccessor<User>>(x => x.GetById(userId.FormatGuid()) == accountDocument);

            var questionnaireRepository = Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>(x => 
                x.GetById(questionnaireId.FormatGuid()) == questionnaireDocument);

            this.translation = new QuestionnaireTranslation(new List<TranslationDto>());

            this.translator = new Mock<IQuestionnaireTranslator>();
            translator.Setup(x => x.Translate(questionnaireDocument, translation))
                .Returns(translatedDocument);

            translationsService = new Mock<ITranslationsService>();
            translationsService
                .Setup(x => x.Get(questionnaireId, translationId))
                .Returns(translation);

            var questionnaireChangeHistoryStorage = new InMemoryPlainStorageAccessor<QuestionnaireChangeRecord>();
            questionnaireChangeHistoryStorage.Store(
                new QuestionnaireChangeRecord
                {
                    QuestionnaireId = questionnaireId.FormatGuid(),
                    UserId = userId,
                    UserName = userName
                }, "");

            var questionnaireListItemStorage = new InMemoryPlainStorageAccessor<QuestionnaireListViewItem>();
            var questionnaireListViewItem = Create.QuestionnaireListViewItem();
            
            questionnaireListItemStorage.Store(questionnaireListViewItem, questionnaireId.FormatGuid());

            factory = CreateFactory(accountsDocumentReader: accountsDocumentReader,
                questionnaireStorage: questionnaireRepository,
                questionnaireChangeHistoryStorage: questionnaireChangeHistoryStorage,
                questionnaireListViewItemStorage: questionnaireListItemStorage,
                translationService: this.translationsService.Object,
                translator: this.translator.Object);
        }

        private void BecauseOf() => view = factory.Load(questionnaireId.FormatGuid(), userId, userName);

        [Test]
        public void should_translate_document_according_to_default_translation()
        {
            questionnaireDocument.DefaultTranslation = translationId;

            BecauseOf();

            this.translationsService
                .Verify(x => x.Get(questionnaireId, translationId), Times.Once);

            this.translator
                .Verify(x => x.Translate(questionnaireDocument, translation), Times.Once);
        }

        [Test]
        public void should_not_translate_document_with_original_translation()
        {
            questionnaireDocument.DefaultTranslation = null;

            BecauseOf();

            this.translationsService
                .Verify(x => x.Get(questionnaireId, translationId), Times.Never);

            this.translator
                .Verify(x => x.Translate(questionnaireDocument, translation), Times.Never);
        }

        private static PdfQuestionnaireModel view;
        private static PdfFactory factory;
        private static Guid questionnaireId = Id.g1;
        private static Guid userId = Id.g2;
        private static Guid translationId = Id.gF;
        private static string userName = "user";
        private static string userEmail = "user@e.mail";
        private QuestionnaireDocument questionnaireDocument;
        private ITranslation translation;
    }
}