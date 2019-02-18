using System;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_updating_roster_group_referencing_text_list_question_as_roster_size_source : QuestionnaireTestsContext
    {
        [NUnit.Framework.Test] public void should_not_fail () {
            questionnaire = CreateQuestionnaire(responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddTextListQuestion(rosterSizeQuestionId, chapterId, responsibleId);
            questionnaire.AddGroup(groupId, chapterId, responsibleId: responsibleId);
            Assert.DoesNotThrow(() =>
                questionnaire.UpdateGroup(
                    groupId, responsibleId, "title",null, rosterSizeQuestionId, "description", null, hideIfDisabled: false,
                    isRoster: true, rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: new FixedRosterTitleItem[0], 
                    rosterTitleQuestionId: null, isPlainMode: false));
        }

        private static Questionnaire questionnaire;

        private static Guid chapterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid groupId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid responsibleId = Guid.Parse("CCCCCCCCCCCCCCCCDDDDDDDDDDDDDDDD");
        private static Guid rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");
    }
}
