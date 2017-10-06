using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_adding_group_under_roster : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddGroup(parentRosterId, chapterId, responsibleId: responsibleId, isRoster: true);
            BecauseOf();

        }

        private void BecauseOf() => questionnaire.AddGroupAndMoveIfNeeded(groupId: groupId, responsibleId: responsibleId, title: title, variableName: null, rosterSizeQuestionId: null, description: description,
                    condition: condition, hideIfDisabled: hideIfDisabled, parentGroupId: parentRosterId, isRoster: false, rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null, index: index);

        [NUnit.Framework.Test] public void should_create_group_with_GroupId_specified () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId).ShouldNotBeNull();

        [NUnit.Framework.Test] public void should_create_group_ConditionExpression_specified () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .ConditionExpression.ShouldEqual(condition);

        [NUnit.Framework.Test] public void should_create_group_with_HideIfDisabled_specified () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .HideIfDisabled.ShouldEqual(hideIfDisabled);

        [NUnit.Framework.Test] public void should_create_group_with_Title_specified () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .Title.ShouldEqual(title);

        [NUnit.Framework.Test] public void should_create_group_with_Description_specified () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .Description.ShouldEqual(description);

        private static Questionnaire questionnaire;
        private static Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static Guid parentRosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static string condition = "some condition";
        private static bool hideIfDisabled = true;
        private static string title = "title";
        private static string description = "description";
        private static int index = 5;
    }
}