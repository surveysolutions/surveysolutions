using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireInfoFactoryTests
{
    internal class when_getting_group_edit_view : QuestionnaireInfoFactoryTestContext
    {
        Establish context = () =>
        {
            questionDetailsReaderMock = new Mock<IReadSideRepositoryReader<QuestionsAndGroupsCollectionView>>();
            questionnaireView = CreateQuestionsAndGroupsCollectionView();
            questionDetailsReaderMock
                .Setup(x => x.GetById(questionnaireId))
                .Returns(questionnaireView);

            factory = CreateQuestionnaireInfoFactory(questionDetailsReaderMock.Object);
        };

        Because of = () =>
            result = factory.GetGroupEditView(questionnaireId, groupId);

        It should_return_not_null_view = () => 
            result.ShouldNotBeNull();

        It should_return_group_with_Id_equals_g4Id = () =>
            result.Group.Id.ShouldEqual(g4Id);

        It should_return_group_with_Title_equals_g4_title = () =>
            result.Group.Title.ShouldEqual(GetGroup(g4Id).Title);

        It should_return_group_with_EnablementCondition_equals_g4_enablementCondition = () =>
            result.Group.EnablementCondition.ShouldEqual(GetGroup(g4Id).EnablementCondition);

        It should_return_group_with_Description_equals_g4_description = () =>
            result.Group.Description.ShouldEqual(GetGroup(g4Id).Description);

        private static GroupAndRosterDetailsView GetGroup(Guid groupId)
        {
            return questionnaireView.Groups.Single(x => x.Id == groupId);
        }

        private static QuestionnaireInfoFactory factory;
        private static NewEditGroupView result;
        private static QuestionsAndGroupsCollectionView questionnaireView;
        private static Mock<IReadSideRepositoryReader<QuestionsAndGroupsCollectionView>> questionDetailsReaderMock;
        private static string questionnaireId = "11111111111111111111111111111111";
        private static Guid groupId = g4Id;
    }
}