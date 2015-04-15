using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireInfoFactoryTests
{
    internal class when_getting_numeric_roster_edit_view : QuestionnaireInfoFactoryTestContext
    {
        Establish context = () =>
        {
            questionDetailsReaderMock = new Mock<IReadSideKeyValueStorage<QuestionsAndGroupsCollectionView>>();
            questionnaireView = CreateQuestionsAndGroupsCollectionView();
            questionDetailsReaderMock
                .Setup(x => x.GetById(questionnaireId))
                .Returns(questionnaireView);

            factory = CreateQuestionnaireInfoFactory(questionDetailsReaderMock.Object);
        };

        Because of = () =>
            result = factory.GetRosterEditView(questionnaireId, rosterId);

        It should_return_not_null_view = () =>
            result.ShouldNotBeNull();

        It should_return_roster_with_ItemId_equals_groupId = () =>
            result.ItemId.ShouldEqual(rosterId.FormatGuid());

        It should_return_roster_with_RosterFixedTitles_count_equals_to_0 = () =>
            result.FixedRosterTitles.Count.ShouldEqual(0);

        It should_return_roster_with_RosterSizeMultiQuestionId_equals_g3_RosterSizeQuestionId = () =>
            result.RosterSizeMultiQuestionId.ShouldEqual(q2Id.FormatGuid());

        It should_return_roster_with_RosterSizeListQuestionId_equals_g3_RosterSizeQuestionId = () =>
            result.RosterSizeListQuestionId.ShouldBeNull();

        It should_return_roster_with_RosterSizeNumericQuestionId_equals_g3_RosterSizeQuestionId = () =>
            result.RosterSizeNumericQuestionId.ShouldBeNull();

        It should_return_roster_with_RosterSizeSourceType_equals_g3_RosterSizeSourceType = () =>
            result.Type.ShouldEqual(RosterType.Multi);
        

        private static QuestionnaireInfoFactory factory;
        private static NewEditRosterView result;
        private static QuestionsAndGroupsCollectionView questionnaireView;
        private static Mock<IReadSideKeyValueStorage<QuestionsAndGroupsCollectionView>> questionDetailsReaderMock;
        private static string questionnaireId = "11111111111111111111111111111111";
        private static Guid rosterId = g2Id;
    }
}