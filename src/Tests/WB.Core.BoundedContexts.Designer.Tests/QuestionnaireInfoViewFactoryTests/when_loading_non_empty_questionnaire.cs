using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireInfoViewFactoryTests
{
    internal class when_loading_non_empty_questionnaire : QuestionnaireInfoViewFactoryContext
    {
        Establish context = () =>
        {
            var repositoryMock = new Mock<IQueryableReadSideRepositoryReader<QuestionnaireInfoView>>();

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

            var questionnaireDocument = Mock.Of<IReadSideRepositoryReader<QuestionnaireDocument>>(x => x.GetById(questionnaireId) == questionnaire);

            factory = CreateQuestionnaireInfoViewFactory(documentReader: questionnaireDocument, repository: repositoryMock.Object);
        };

        Because of = () => view = factory.Load(questionnaireId);

        It should_count_number_of_questions_in_questionnaire = () => view.QuestionsCount.ShouldEqual(1);

        It should_count_number_of_groups_in_questionnaire = () => view.GroupsCount.ShouldEqual(1);

        It should_count_number_of_roster_in_questionnaire =() => view.RostersCount.ShouldEqual(1);

        private static QuestionnaireInfoView view;
        private static QuestionnaireInfoViewFactory factory;
        private static string questionnaireId = "11111111111111111111111111111111";
        private static string questionnaireTitle = "questionnaire title";
    }
}