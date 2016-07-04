using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.ChapterInfoViewDenormalizerTests
{
    internal class when_VariableCloned_event_received : ChaptersInfoViewDenormalizerTestContext
    {
        Establish context = () =>
        {
            viewState = CreateGroupInfoViewWith1ChapterAnd3QuestionInsideChapter(chapterId, question1Id, question2Id, question3Id);
            denormalizer = CreateDenormalizer(view: viewState);
        };

        Because of = () =>
            viewState = denormalizer.Update(viewState, 
                    Create.Event.VariableClonedEvent(entityId: variableId, targetIndex:1, parentId: chapterId, variableName: variableName));


        It should_groupInfoView_first_chapter_items_not_be_null = () =>
            ((GroupInfoView)viewState.Items[0]).Items.ShouldNotBeNull();

        It should_groupInfoView_first_chapter_items_count_be_equal_to_4 = () =>
            ((GroupInfoView)viewState.Items[0]).Items.Count.ShouldEqual(4);

        It should_groupInfoView_first_chapter_first_item_type_be_equal_to_QuestionInfoView = () =>
            ((GroupInfoView)viewState.Items[0]).Items[1].ShouldBeOfExactType<VariableView>();

        It should_groupInfoView_first_chapter_first_item_id_be_equal_to_questionId = () =>
            ((GroupInfoView)viewState.Items[0]).Items[1].ItemId.ShouldEqual(variableId);

        private static string chapterId = "33333333333333333333333333333333";
        private static string variableId = "22222222222222222222222222222222";
        private static string question1Id = "11111111111111111111111111111111";

        private static string question2Id = "11111111111111111111111111111112";
        private static string question3Id = "11111111111111111111111111111113";

        private static string variableName = "var";
        
        private static ChaptersInfoViewDenormalizer denormalizer;
        private static GroupInfoView viewState;
    }
}
