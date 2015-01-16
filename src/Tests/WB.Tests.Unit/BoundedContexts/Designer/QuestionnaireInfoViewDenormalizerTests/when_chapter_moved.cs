using System;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.GenericSubdomains.Utils;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireInfoViewDenormalizerTests
{
    internal class when_chapter_moved : QuestionnaireInfoViewDenormalizerTestContext
    {
        Establish context = () =>
        {
            viewState = CreateQuestionnaireInfoViewWith1Chapter(questionnaireId);
            denormalizer = CreateDenormalizer(viewState);
            movedItemId = Guid.NewGuid().FormatGuid();

            viewState = denormalizer.Update(null, Create.NewQuestionnaireCreatedEvent(questionnaireId));
            denormalizer.Update(viewState, Create.NewGroupAddedEvent(groupId: groupId, parentGroupId: questionnaireId, groupTitle: "group title"));
            denormalizer.Update(viewState, Create.NewGroupAddedEvent(groupId: movedItemId, parentGroupId: questionnaireId, groupTitle: "group title to move"));
        };

        Because of = () =>
            viewState =
                denormalizer.Update(viewState, Create.QuestionnaireItemMovedEvent(itemId: movedItemId));

        It should_move_chapter = () => viewState.Chapters[0].ItemId.ShouldEqual(movedItemId);

        It should_keep_amout_of_chapters_the_same = () => viewState.Chapters.Count.ShouldEqual(2);

        private static string questionnaireId = "33333333333333333333333333333333";
        private static string groupId = "22222222222222222222222222222222";

        private static QuestionnaireInfoViewDenormalizer denormalizer;
        private static QuestionnaireInfoView viewState;
        private static string movedItemId;
    }
}

