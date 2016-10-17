using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_updating_roster_group_by_fixed_titles_and_have_more_than_250_titles : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            parentGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            groupId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAB");
            rosterSizeSourceType = RosterSizeSourceType.FixedTitles;
            rosterFixedTitles = Enumerable.Range(1, 251).Select(i => new FixedRosterTitleItem(i++.ToString(), i.ToString())).ToArray();
            
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddTextQuestion(questionId, chapterId, responsibleId);
            
            questionnaire.AddGroup(parentGroupId, responsibleId: responsibleId);
            questionnaire.AddGroup(groupId, parentGroupId, responsibleId: responsibleId);
        };

        Because of = () =>
            exception = Catch.Exception(
                () =>
                    questionnaire.UpdateGroup(groupId: groupId, responsibleId: responsibleId, title: "title", variableName: null,
                        description: null, condition: null, hideIfDisabled: false, rosterSizeQuestionId: null, isRoster: true,
                        rosterSizeSource: rosterSizeSourceType, rosterFixedTitles: rosterFixedTitles, rosterTitleQuestionId: null));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();


        It should_throw_exception_with_message_containting__fixed__ = () =>
            exception.Message.ToLower().ShouldContain("fixed");

        It should_throw_exception_with_message_containting__titles__ = () =>
            exception.Message.ToLower().ShouldContain("titles");

        It should_throw_exception_with_message_containting__more__ = () =>
            exception.Message.ToLower().ShouldContain("more");

        It should_throw_exception_with_message_containting__250__ = () =>
            exception.Message.ToLower().ShouldContain("250");

        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid groupId;
        private static Guid parentGroupId;
        private static RosterSizeSourceType rosterSizeSourceType;
        private static FixedRosterTitleItem[] rosterFixedTitles;
        private static Exception exception;
    }
}