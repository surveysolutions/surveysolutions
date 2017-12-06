using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_adding_roster_under_roster : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddNumericQuestion(rosterSizeQuestionId, chapterId,responsibleId, isInteger: true);
            questionnaire.AddGroup(parentRosterId, chapterId, responsibleId: responsibleId, isRoster: true);
            BecauseOf();

        }


        private void BecauseOf() => questionnaire.AddGroupAndMoveIfNeeded(groupId: groupId, responsibleId: responsibleId, title: title, variableName: null, 
                    rosterSizeQuestionId: rosterSizeQuestionId, description: description, condition: null, hideIfDisabled: false, parentGroupId: parentRosterId, isRoster: true, rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);

        [NUnit.Framework.Test] public void should_contains_group () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId).ShouldNotBeNull();

        [NUnit.Framework.Test] public void should_contains_group_with_GroupId_specified () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .PublicKey.ShouldEqual(groupId);

        [NUnit.Framework.Test] public void should_contains_group_with_ParentGroupId_specified () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .GetParent().PublicKey.ShouldEqual(parentRosterId);

        [NUnit.Framework.Test] public void should_contains_group_with_Title_specified () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .Title.ShouldEqual(title);

        [NUnit.Framework.Test] public void should_contains_group_with_Description_specified () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .Description.ShouldEqual(description);


        private static Questionnaire questionnaire;
        private static Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static Guid parentRosterId=Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid rosterSizeQuestionId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static string title = "title";
        private static string description = "description";
    }
}