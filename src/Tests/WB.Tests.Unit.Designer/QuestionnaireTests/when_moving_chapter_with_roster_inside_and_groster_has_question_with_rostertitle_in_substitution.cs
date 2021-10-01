using System;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_moving_chapter_with_roster_inside_and_groster_has_question_with_rostertitle_in_substitution : QuestionnaireTestsContext
    {
        [NUnit.Framework.Test] public void should_not_throw_QuestionnaireException () {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterBId, responsibleId: responsibleId);
            questionnaire.AddGroup( chapterAId, responsibleId: responsibleId);
            questionnaire.AddGroup(rosterId,  chapterAId, responsibleId: responsibleId, isRoster: true);
            
            questionnaire.AddTextQuestion(questionWithSubstitutionId,
                rosterId,
                responsibleId,
                variableName: "var",
                title: "%rostertitle% hello"
            );

            // act
            Assert.DoesNotThrow(() 
                => questionnaire.MoveGroup(chapterAId, null, 1, responsibleId));
        }

        private static Exception exception;
        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid chapterAId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid chapterBId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid rosterId = Guid.Parse("11111111111111111111111111111112");
        private static Guid questionWithSubstitutionId = Guid.Parse("44444444444444444444444444444444");
    }
}
