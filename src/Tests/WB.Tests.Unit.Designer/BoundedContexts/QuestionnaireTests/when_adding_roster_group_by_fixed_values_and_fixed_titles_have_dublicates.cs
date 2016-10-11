using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_adding_roster_group_by_fixed_values_and_fixed_titles_have_dublicates : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            parentGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            groupId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            rosterSizeSourceType = RosterSizeSourceType.FixedTitles;
            rosterFixedTitles = new[] { new FixedRosterTitleItem("1", "fixed title 1"), 
                                        new FixedRosterTitleItem("1", "duplicate"), 
                                        new FixedRosterTitleItem("3", "fixed title 3") };

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddTextQuestion(Guid.NewGuid(),
                 chapterId,
                responsibleId);
            questionnaire.AddGroup(parentGroupId, responsibleId: responsibleId);
        };

        Because of = () =>
            exception = Catch.Exception(
                () =>
                    questionnaire.AddGroupAndMoveIfNeeded(groupId: groupId, responsibleId: responsibleId, title: "title", variableName: null, rosterSizeQuestionId: null, description: null,
                        condition: null, hideIfDisabled: false, parentGroupId: parentGroupId, isRoster: true, rosterSizeSource: rosterSizeSourceType, rosterFixedTitles: rosterFixedTitles, rosterTitleQuestionId: null));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();
        
        It should_throw_exception_with_message = () =>
            new[] { "must", "unique", "value" }.ShouldEachConformTo(keyword => exception.Message.ToLower().Contains(keyword));
        
        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid groupId;
        private static Guid parentGroupId;
        private static RosterSizeSourceType rosterSizeSourceType;
        private static FixedRosterTitleItem[] rosterFixedTitles;
        private static Exception exception;
    }
}
