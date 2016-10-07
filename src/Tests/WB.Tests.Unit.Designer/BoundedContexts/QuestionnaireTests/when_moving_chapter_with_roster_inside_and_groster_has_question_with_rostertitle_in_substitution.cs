using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_moving_chapter_with_roster_inside_and_groster_has_question_with_rostertitle_in_substitution : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterBId });
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterAId });
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = rosterId, ParentGroupPublicKey = chapterAId });
            questionnaire.MarkGroupAsRoster(new GroupBecameARoster(responsibleId, rosterId));
            questionnaire.AddTextQuestion(questionWithSubstitutionId,
                rosterId,
                responsibleId,
                variableName: "var",
                title: "%rostertitle% hello"
            );
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.MoveGroup(chapterAId, null, 0, responsibleId));

        It should_not_throw_QuestionnaireException = () =>
            exception.ShouldBeNull();

        private static Exception exception;
        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid chapterAId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid chapterBId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid rosterId = Guid.Parse("11111111111111111111111111111111");
        private static Guid questionWithSubstitutionId = Guid.Parse("44444444444444444444444444444444");
    }
}