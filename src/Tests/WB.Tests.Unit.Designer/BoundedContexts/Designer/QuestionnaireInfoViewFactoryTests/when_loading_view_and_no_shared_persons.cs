using System;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireInfoViewFactoryTests
{
    internal class when_loading_view_and_no_shared_persons : QuestionnaireInfoViewFactoryContext
    {
        Establish context = () =>
        {
            var questionnaireInfoViewRepository = Mock.Of<IReadSideKeyValueStorage<QuestionnaireInfoView>>(
                x => x.GetById(questionnaireId) == CreateQuestionnaireInfoView(questionnaireId, questionnaireTitle));

            var questionnaireDocument = Create.QuestionnaireDocumentWithSharedPersons(Guid.Parse(questionnaireId), userId);

            var questionnaireDocumentRepository = Mock.Of<IReadSideKeyValueStorage<QuestionnaireDocument>>(
                x => x.GetById(questionnaireId) == questionnaireDocument);

            var accountDocument = new AccountDocument { Email = userEmail };
            var accountDocumentRepository = Mock.Of<IReadSideRepositoryReader<AccountDocument>>(
                x => x.GetById(userId.FormatGuid()) == accountDocument);

            factory = CreateQuestionnaireInfoViewFactory(repository: questionnaireInfoViewRepository,
                documentReader: questionnaireDocumentRepository, accountsDocumentReader: accountDocumentRepository);
        };

        Because of = () => view = factory.Load(questionnaireId, userId);

        It should_be_only_owner_in_shared_persons_list = () =>
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