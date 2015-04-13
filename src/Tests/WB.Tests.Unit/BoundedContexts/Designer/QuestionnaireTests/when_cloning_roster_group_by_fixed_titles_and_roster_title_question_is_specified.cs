using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_cloning_roster_group_by_fixed_titles_and_roster_title_question_is_specified : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            parentGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            groupId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            newGroupId = Guid.Parse("2BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            rosterTitleQuestionId = Guid.Parse("1BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            rosterSizeSourceType = RosterSizeSourceType.FixedTitles;
            rosterFixedTitles = new[] { new Tuple<string, string>("1","fixed title 1"), 
                new Tuple<string, string>("2","fixed title 2"), new Tuple<string, string>("3","fixed title 3") };

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
                        description: null, condition: null, rosterSizeQuestionId: null, isRoster: true,
                        rosterSizeSource: rosterSizeSourceType, rosterFixedTitles: rosterFixedTitles,
                        rosterTitleQuestionId: rosterTitleQuestionId, targetIndex: 0, parentGroupId: parentGroupId, sourceGroupId: groupId));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();


        It should_throw_exception_with_message_containting__have__ = () =>
            exception.Message.ToLower().ShouldContain("have");

        It should_throw_exception_with_message_containting__roster__ = () =>
            exception.Message.ToLower().ShouldContain("roster");

        It should_throw_exception_with_message_containting__size__ = () =>
            exception.Message.ToLower().ShouldContain("title");

        It should_throw_exception_with_message_containting__question__ = () =>
            exception.Message.ToLower().ShouldContain("question");

        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid groupId;
        private static Guid newGroupId;
        private static Guid parentGroupId;
        private static RosterSizeSourceType rosterSizeSourceType;
        private static Tuple<string, string>[] rosterFixedTitles;
        private static Guid rosterTitleQuestionId;
        private static Exception exception;
    }
}