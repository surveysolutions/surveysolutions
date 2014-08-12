using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    internal class when_moving_chapter_with_roster_inside_and_groster_has_question_with_rostertitle_in_substitution : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterBId });
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterAId });
            questionnaire.Apply(new NewGroupAdded { PublicKey = rosterId, ParentGroupPublicKey = chapterAId });
            questionnaire.Apply(new GroupBecameARoster(responsibleId, rosterId));
            questionnaire.Apply(new NewQuestionAdded()
            {
                PublicKey = questionWithSubstitutionId,
                GroupPublicKey = rosterId,
                QuestionType = QuestionType.Text,
                QuestionText = "%rostertitle% hello",
                StataExportCaption = "var"
            });
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