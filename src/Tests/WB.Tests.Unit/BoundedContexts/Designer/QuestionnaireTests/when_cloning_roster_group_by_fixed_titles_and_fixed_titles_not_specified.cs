using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_cloning_roster_group_by_fixed_titles_and_fixed_titles_not_specified : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            parentGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            groupId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            newGroupId = Guid.Parse("1BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            rosterSizeSourceType = RosterSizeSourceType.FixedTitles;

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NewGroupAdded { PublicKey = groupId, ParentGroupPublicKey = chapterId });
            questionnaire.Apply(new NewQuestionAdded()
            {
                PublicKey = Guid.NewGuid(),
                GroupPublicKey = chapterId,
                QuestionType = QuestionType.Text
            });
            questionnaire.Apply(new NewGroupAdded { PublicKey = parentGroupId });
        };

        Because of = () =>
            exception = Catch.Exception(
                () =>
                    questionnaire.CloneGroupWithoutChildren(groupId: newGroupId, responsibleId: responsibleId, title: "title", variableName: null,
                        parentGroupId: parentGroupId, description: null, condition: null, rosterSizeQuestionId: null, isRoster: true,
                        rosterSizeSource: rosterSizeSourceType, rosterFixedTitles: null, rosterTitleQuestionId: null, targetIndex: 0,
                        sourceGroupId: groupId));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();
        It should_throw_exception_with_message = () =>
            new[] { "not", "be", "empty"}.ShouldEachConformTo(keyword => exception.Message.ToLower().Contains(keyword));
       
        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid groupId;
        private static Guid newGroupId;
        private static Guid parentGroupId;
        private static RosterSizeSourceType rosterSizeSourceType;
        private static Exception exception;
    }
}