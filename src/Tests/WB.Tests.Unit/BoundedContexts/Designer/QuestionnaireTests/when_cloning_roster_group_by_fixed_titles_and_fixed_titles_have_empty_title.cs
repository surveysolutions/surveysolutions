using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_cloning_roster_group_by_fixed_titles_and_fixed_titles_have_empty_title : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            parentGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            groupId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            newGroupId = Guid.Parse("1BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            rosterSizeSourceType = RosterSizeSourceType.FixedTitles;
            rosterFixedTitles = new[] { new Tuple<string, string>("1","fixed title 1"), 
                                        new Tuple<string, string>("2",  " "),
                                        new Tuple<string, string>("3", "fixed title 3") };

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
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
                        rosterSizeSource: rosterSizeSourceType, rosterFixedTitles: rosterFixedTitles, rosterTitleQuestionId: null,
                        targetIndex: 0, sourceGroupId: groupId));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message = () =>
            new[] { "not", "empty", "title"}.ShouldEachConformTo(keyword => exception.Message.ToLower().Contains(keyword));
       

        It should_throw_exception_with_message_containting__not__ = () =>
            exception.Message.ToLower().ShouldContain("not");

        It should_throw_exception_with_message_containting__empty__ = () =>
            exception.Message.ToLower().ShouldContain("empty");

        It should_throw_exception_with_message_containting__title__ = () =>
            exception.Message.ToLower().ShouldContain("title");

        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid groupId;
        private static Guid newGroupId;
        private static Guid parentGroupId;
        private static RosterSizeSourceType rosterSizeSourceType;
        private static Tuple<string, string>[] rosterFixedTitles;
        private static Exception exception;
    }
}