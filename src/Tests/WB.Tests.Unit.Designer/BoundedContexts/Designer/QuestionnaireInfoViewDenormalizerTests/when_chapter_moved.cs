using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireInfoViewDenormalizerTests
{
    internal class when_chapter_moved : QuestionnaireInfoViewDenormalizerTestContext
    {
        Establish context = () =>
        {
            viewState = CreateQuestionnaireInfoViewWith1Chapter(questionnaireId);
            denormalizer = CreateDenormalizer(viewState);
            movedItemId = Guid.NewGuid().FormatGuid();

            viewState = denormalizer.Update(null, Create.Event.NewQuestionnaireCreatedEvent(questionnaireId));
            denormalizer.Update(viewState, Create.Event.NewGroupAddedEvent(groupId: groupId, parentGroupId: questionnaireId, groupTitle: "group title"));
            denormalizer.Update(viewState, Create.Event.NewGroupAddedEvent(groupId: movedItemId, parentGroupId: questionnaireId, groupTitle: "group title to move"));
        };

        Because of = () =>
            viewState =
                denormalizer.Update(viewState, Create.Event.QuestionnaireItemMovedEvent(itemId: movedItemId));

        It should_move_chapter = () => viewState.Chapters[0].ItemId.ShouldEqual(movedItemId);

        It should_keep_amout_of_chapters_the_same = () => viewState.Chapters.Count.ShouldEqual(2);

        private static string questionnaireId = "33333333333333333333333333333333";
        private static string groupId = "22222222222222222222222222222222";

        private static QuestionnaireInfoViewDenormalizer denormalizer;
        private static QuestionnaireInfoView viewState;
        private static string movedItemId;
    }
}

