using System;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireInfoViewFactoryTests
{
    internal class when_loading_view_and_shared_persons_contains_viewer : QuestionnaireInfoViewFactoryContext
    {
        Establish context = () =>
        {
            var questionnaireInfoViewRepository = Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>(
                x => x.GetById(questionnaireId) == CreateQuestionnaireDocument(questionnaireId, questionnaireTitle));

            var questionnaireSharedPersons = new QuestionnaireSharedPersons(Guid.Parse(questionnaireId));
            questionnaireSharedPersons.SharedPersons.Add(new SharedPerson
            {
                Id = userId,
                Email = userEmail,
                IsOwner = false
            });
            var sharedPersonsRepository = Mock.Of<IPlainKeyValueStorage<QuestionnaireSharedPersons>>(
                x => x.GetById(Moq.It.IsAny<string>()) == questionnaireSharedPersons);

            var accountDocument = new User { Email = userEmail };
            var accountDocumentRepository = Mock.Of<IPlainStorageAccessor<User>>(
                x => x.GetById(userId.FormatGuid()) == accountDocument);

            factory = CreateQuestionnaireInfoViewFactory(repository: questionnaireInfoViewRepository,
                sharedWith: sharedPersonsRepository, accountsDocumentReader: accountDocumentRepository);
        };

        Because of = () => view = factory.Load(questionnaireId, userId);

        It should_be_only_1_specified_shared_person = () =>
        {
            view.SharedPersons.Count.ShouldEqual(1);
            view.SharedPersons[0].Email.ShouldEqual(userEmail);
        };

        private static QuestionnaireInfoView view;
        private static QuestionnaireInfoViewFactory factory;
        private static string questionnaireId = "11111111111111111111111111111111";
        private static string questionnaireTitle = "questionnaire title";
        private static readonly Guid userId = Guid.Parse("22222222222222222222222222222222");
        private static string userEmail = "user@email.com";
    }
}