using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_updating_group_and_is_roster_flag_set_to_false : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            questionnaire = CreateQuestionnaireWithOneGroup(responsibleId, groupId: groupId);
            BecauseOf();
        }

        private void BecauseOf() =>
            questionnaire.UpdateGroup(groupId, responsibleId, "title",null, null, null, null, hideIfDisabled: false, isRoster: false,
                rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);


        [NUnit.Framework.Test] public void should_contains_group () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId).ShouldNotBeNull();

        [NUnit.Framework.Test] public void should_contains_group_with_GroupId_specified () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .PublicKey.ShouldEqual(groupId);

        [NUnit.Framework.Test] public void should_contains_group_with_IsRoster_specified () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .IsRoster.ShouldBeFalse();

        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid groupId;
    }
}