using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireInfoFactoryTests
{
    internal class when_getting_roster_edit_view_and_questionnaire_is_absent : QuestionnaireInfoFactoryTestContext
    {
        Establish context = () =>
        {
            questionDetailsReaderMock = new Mock<IReadSideKeyValueStorage<QuestionsAndGroupsCollectionView>>();
            factory = CreateQuestionnaireInfoFactory(questionDetailsReaderMock.Object);
        };

        Because of = () =>
            result = factory.GetRosterEditView(questionnaireId, rosterId);

        It should_return_null = () =>
            result.ShouldBeNull();

        private static QuestionnaireInfoFactory factory;
        private static NewEditRosterView result;
        private static Mock<IReadSideKeyValueStorage<QuestionsAndGroupsCollectionView>> questionDetailsReaderMock;
        private static string questionnaireId = "11111111111111111111111111111111";
        private static Guid rosterId = g4Id;
    }
}