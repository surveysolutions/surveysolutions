using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
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
    [TestOf(typeof(PdfFactory))]
    internal class PdfFactoryTests : PdfFactoryTestsContext
    {
        [Test]
        public void when_load_pdf_factory_with_localized_questionnaire_should_translate_document_according_to_default_translation()
        {
            //arrange
            Guid questionnaireId = Id.g1;
            Guid userId = Id.g2;
            Guid translationId = Id.gF;
            string userName = "user";
            string userEmail = "user@e.mail";

            var accountDocument = Create.AccountDocument(userName);

            var questionnaireDocument = Create.QuestionnaireDocument(questionnaireId, title: "Na Movi");
            var translatedDocument = Create.QuestionnaireDocument(questionnaireId, title: "Íà ìîâè");

            var accountsDocumentReader = Mock.Of<IPlainStorageAccessor<User>>(x => x.GetById(userId.FormatGuid()) == accountDocument);

            var questionnaireRepository = Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>(x =>
                x.GetById(questionnaireId.FormatGuid()) == questionnaireDocument);

            var translation = new QuestionnaireTranslation(new List<TranslationDto>());

            var translator = new Mock<IQuestionnaireTranslator>();
            translator.Setup(x => x.Translate(questionnaireDocument, translation)).Returns(translatedDocument);

            var translationsService = new Mock<ITranslationsService>();
            translationsService.Setup(x => x.Get(questionnaireId, translationId)).Returns(translation);

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

            var factory = CreateFactory(accountsDocumentReader: accountsDocumentReader,
                questionnaireStorage: questionnaireRepository,
                questionnaireChangeHistoryStorage: questionnaireChangeHistoryStorage,
                questionnaireListViewItemStorage: questionnaireListItemStorage,
                translationService: translationsService.Object,
                questionnaireTranslator: translator.Object);

            questionnaireDocument.DefaultTranslation = translationId;

            //act
            var view = factory.Load(questionnaireId.FormatGuid(), userId, userName, null, true);

            //assert
            translationsService.Verify(x => x.Get(questionnaireId, translationId), Times.Once);
            translator.Verify(x => x.Translate(questionnaireDocument, translation), Times.Once);
        }

        [Test]
        public void when_load_pdf_factory_with_localized_questionnaire_should_not_translate_document_with_original_translation()
        {
            //arrange
            Guid questionnaireId = Id.g1;
            Guid userId = Id.g2;
            Guid translationId = Id.gF;
            string userName = "user";
            string userEmail = "user@e.mail";

            var accountDocument = Create.AccountDocument(userName);

            var questionnaireDocument = Create.QuestionnaireDocument(questionnaireId, title: "Na Movi");
            var translatedDocument = Create.QuestionnaireDocument(questionnaireId, title: "Íà ìîâè");

            var accountsDocumentReader = Mock.Of<IPlainStorageAccessor<User>>(x => x.GetById(userId.FormatGuid()) == accountDocument);

            var questionnaireRepository = Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>(x =>
                x.GetById(questionnaireId.FormatGuid()) == questionnaireDocument);

            var translation = new QuestionnaireTranslation(new List<TranslationDto>());

            var translator = new Mock<IQuestionnaireTranslator>();
            translator.Setup(x => x.Translate(questionnaireDocument, translation)).Returns(translatedDocument);

            var translationsService = new Mock<ITranslationsService>();
            translationsService.Setup(x => x.Get(questionnaireId, translationId)).Returns(translation);

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

            var factory = CreateFactory(accountsDocumentReader: accountsDocumentReader,
                questionnaireStorage: questionnaireRepository,
                questionnaireChangeHistoryStorage: questionnaireChangeHistoryStorage,
                questionnaireListViewItemStorage: questionnaireListItemStorage,
                translationService: translationsService.Object,
                questionnaireTranslator: translator.Object);

            questionnaireDocument.DefaultTranslation = null;

            //act
            var view = factory.Load(questionnaireId.FormatGuid(), userId, userName, null, true);

            //assert
            translationsService.Verify(x => x.Get(questionnaireId, translationId), Times.Never);
            translator.Verify(x => x.Translate(questionnaireDocument, translation), Times.Never);
        }

        [Test]
        public void when_load_and_shared_persons_contains_requested_user()
        {
            //arrange
            string userName = "user";
            string userEmail = "user@e.mail";

            Guid userId = Id.g2;
            Guid questionnaireId = Id.g1;

            var accountDocument = Create.AccountDocument(userName);
            var questionnaireDocument = Create.QuestionnaireDocument();

            var accountsDocumentReader = Mock.Of<IPlainStorageAccessor<User>>(x => x.GetById(userId.FormatGuid()) == accountDocument);
            var questionnaireRepository = Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>(x => x.GetById(questionnaireId.FormatGuid()) == questionnaireDocument);
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
            questionnaireListViewItem.SharedPersons.Add(Create.SharedPerson(id: userId, email: userEmail));
            questionnaireListItemStorage.Store(questionnaireListViewItem, questionnaireId.FormatGuid());

            var factory = CreateFactory(accountsDocumentReader: accountsDocumentReader,
                questionnaireStorage: questionnaireRepository,
                questionnaireChangeHistoryStorage: questionnaireChangeHistoryStorage,
                questionnaireListViewItemStorage: questionnaireListItemStorage);

            //act
            var view = factory.Load(questionnaireId.FormatGuid(), userId, userName, null, true);

            //assert
            Assert.True(view.SharedPersons.Any());
        }

        [Test]
        public void when_load_and_shared_persons_contains_requested_user_who_is_creator_of_questionnaire()
        {
            //arrange
            string userName = "user";
            string userEmail = "user@e.mail";

            Guid userId = Id.g2;
            Guid questionnaireId = Id.g1;

            var accountDocument = Create.AccountDocument(userName);
            var questionnaireDocument = Create.QuestionnaireDocument(userId: userId);

            var accountsDocumentReader = Mock.Of<IPlainStorageAccessor<User>>(x => x.GetById(userId.FormatGuid()) == accountDocument);
            var questionnaireRepository = Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>(x => x.GetById(questionnaireId.FormatGuid()) == questionnaireDocument);
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
            questionnaireListViewItem.SharedPersons.Add(Create.SharedPerson(id: userId, email: userEmail));
            questionnaireListItemStorage.Store(questionnaireListViewItem, questionnaireId.FormatGuid());

            var factory = CreateFactory(accountsDocumentReader: accountsDocumentReader,
                questionnaireStorage: questionnaireRepository,
                questionnaireChangeHistoryStorage: questionnaireChangeHistoryStorage,
                questionnaireListViewItemStorage: questionnaireListItemStorage);

            //act
            var view = factory.Load(questionnaireId.FormatGuid(), userId, userName, null, true);

            //assert
            Assert.False(view.SharedPersons.Any());
        }
    }
}