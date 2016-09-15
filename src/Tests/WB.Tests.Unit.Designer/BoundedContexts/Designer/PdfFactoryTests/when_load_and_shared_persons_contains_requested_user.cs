using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.PdfFactoryTests
{
    public class when_load_and_shared_persons_contains_requested_user : PdfFactoryTestsContext
    {
        Establish context = () =>
        {
            var accountDocument = Create.AccountDocument(userName);
            var questionnaireDocument = Create.QuestionnaireDocument();
            var questionnaireSharedPersons = Create.QuestionnaireSharedPersons(questionnaireId);
            questionnaireSharedPersons.SharedPersons.Add(Create.SharedPerson(id: userId, email: userEmail));
            var modificationStatisticsByUser = new PdfQuestionnaireModel.ModificationStatisticsByUser {Date = DateTime.Now};

            var accountsDocumentReader = Mock.Of<IReadSideRepositoryReader<AccountDocument>>(x => x.GetById(userId.FormatGuid()) == accountDocument);
            var questionnaireRepository = Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>(x=>x.GetById(questionnaireId.FormatGuid()) == questionnaireDocument);
            var sharedPersonsRepository = Mock.Of<IPlainKeyValueStorage<QuestionnaireSharedPersons>>(x=>x.GetById(questionnaireId.FormatGuid()) == questionnaireSharedPersons);
            var questionnaireChangeHistoryStorage = new TestInMemoryWriter<QuestionnaireChangeRecord>();
            questionnaireChangeHistoryStorage.Store(
                new QuestionnaireChangeRecord
                {
                    QuestionnaireId = questionnaireId.FormatGuid(),
                    UserId = userId,
                    UserName = userName
                }, "");

            factory = CreateFactory(accountsDocumentReader: accountsDocumentReader,
                questionnaireStorage: questionnaireRepository, sharedPersonsStorage: sharedPersonsRepository,
                questionnaireChangeHistoryStorage: questionnaireChangeHistoryStorage);
        };

        Because of = () =>
            view = factory.Load(questionnaireId.FormatGuid(), userId, userName);

        It should_shared_persons_be_empty = () =>
            view.SharedPersons.ShouldBeEmpty();

        private static PdfQuestionnaireModel view;
        private static PdfFactory factory;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static Guid userId = Guid.Parse("22222222222222222222222222222222");
        private static string userName = "user";
        private static string userEmail = "user@e.mail";
    }
}