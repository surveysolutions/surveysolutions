using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_updating_group_and_title_contains_substitution_to_question_from_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddGroup(rosterId, responsibleId: responsibleId);
            questionnaire.MarkGroupAsRoster(Create.Event.GroupBecameRoster(rosterId));
            questionnaire.AddTextQuestion(textQuestionId, rosterId, responsibleId);
            questionnaire.UpdateQuestion(Create.Event.QuestionChanged(publicKey: textQuestionId, groupPublicKey: rosterId, stataExportCaption: textQuestionVariable));
            questionnaire.AddGroup(groupId, chapterId, responsibleId: responsibleId);

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () => exception =
            Catch.Exception(() => questionnaire.UpdateGroup(
                groupId: groupId,
                responsibleId: responsibleId,
                title: $"title %{textQuestionVariable}%",
                variableName: null,
                rosterSizeQuestionId: null,
                description: null,
                condition: null,
                hideIfDisabled: false,
                isRoster: false,
                rosterSizeSource: RosterSizeSourceType.Question,
                rosterFixedTitles: null,
                rosterTitleQuestionId: null));

        It should_exception_has_specified_message = () =>
            new[] { "illegal", "substitution", "to", textQuestionVariable }.ShouldEachConformTo(x =>
                ((QuestionnaireException) exception).Message.Contains(x));

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid rosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid textQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static string textQuestionVariable = "hhname";
        private static Exception exception;
    }
}