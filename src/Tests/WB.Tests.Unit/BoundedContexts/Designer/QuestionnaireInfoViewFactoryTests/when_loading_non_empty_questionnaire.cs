using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireInfoViewFactoryTests
{
    internal class when_loading_non_empty_questionnaire : QuestionnaireInfoViewFactoryContext
    {
        Establish context = () =>
        {
            var repositoryMock = new Mock<IReadSideKeyValueStorage<QuestionnaireInfoView>>();

            repositoryMock
                .Setup(x => x.GetById(questionnaireId))
                .Returns(CreateQuestionnaireInfoView(questionnaireId, questionnaireTitle));

            var questionnaire = Create.CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion
                {
                    QuestionType = QuestionType.Numeric
                },
                new Group("Roster")
                {
                    IsRoster = true
                });

            questionnaire.CreatedBy = userId;

            var questionnaireDocument = Mock.Of<IReadSideKeyValueStorage<QuestionnaireDocument>>(x => x.GetById(questionnaireId) == questionnaire);
            var userRepositoryMock =
                Mock.Of<IReadSideRepositoryReader<AccountDocument>>(
                    x => x.GetById(userId.FormatGuid()) == new AccountDocument() { Email = ownerEmail });
            factory = CreateQuestionnaireInfoViewFactory(documentReader: questionnaireDocument, repository: repositoryMock.Object,
                accountsDocumentReader: userRepositoryMock);
        };

        Because of = () => view = factory.Load(questionnaireId);

        It should_count_number_of_questions_in_questionnaire = () => view.QuestionsCount.ShouldEqual(1);

        It should_count_number_of_groups_in_questionnaire = () => view.GroupsCount.ShouldEqual(1);

        It should_count_number_of_roster_in_questionnaire =() => view.RostersCount.ShouldEqual(1);

        It should_contain_email_of_first_element_in_list_of_shared_persons_equal_to_owner_email = () => view.SharedPersons[0].Email.ShouldEqual(ownerEmail);

        It should_contain_id_of_first_element_in_list_of_shared_persons_equal_to_owner_id = () => view.SharedPersons[0].Id.ShouldEqual(userId);

        It should_contain_isOowner_of_first_element_in_list_of_shared_persons_be_true = () => view.SharedPersons[0].IsOwner.ShouldEqual(true);

        private static QuestionnaireInfoView view;
        private static QuestionnaireInfoViewFactory factory;
        private static string questionnaireId = "11111111111111111111111111111111";
        private static string questionnaireTitle = "questionnaire title";
        private static Guid userId = Guid.Parse("22222222222222222222222222222222");
        private static string ownerEmail= "r@example.org";
    }
}