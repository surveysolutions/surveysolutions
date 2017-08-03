using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_updating_group_and_title_contains_substitution_to_variable_from_roster : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddGroup(rosterId, responsibleId: responsibleId, isRoster:true);
           
            questionnaire.AddVariable(variableId, rosterId, responsibleId, variableName: variableName);
            questionnaire.AddGroup(groupId, chapterId, responsibleId: responsibleId);


            BecauseOf();
        }


        private void BecauseOf() => exception =
            Catch.Exception(() => questionnaire.UpdateGroup(
                groupId: groupId,
                responsibleId: responsibleId,
                title: $"title %{variableName}%",
                variableName: null,
                rosterSizeQuestionId: null,
                description: null,
                condition: null,
                hideIfDisabled: false,
                isRoster: false,
                rosterSizeSource: RosterSizeSourceType.Question,
                rosterFixedTitles: null,
                rosterTitleQuestionId: null));

        [NUnit.Framework.Test] public void should_exception_has_specified_message () =>
            new[] { "illegal", "substitution", "to", variableName }.ShouldEachConformTo(x =>
                ((QuestionnaireException) exception).Message.Contains(x));

        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid rosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid variableId = Guid.Parse("22222222222222222222222222222222");
        private static string variableName = "hhname";
        private static Exception exception;
    }
}