﻿using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_adding_roster_group_referencing_text_list_question_as_roster_size_source : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(Create.Event.NewQuestionAdded (questionType : QuestionType.TextList, publicKey : rosterSizeQuestionId, groupPublicKey : chapterId ));
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.AddGroupAndMoveIfNeeded(
                    groupId, responsibleId, "title",null, rosterSizeQuestionId, "description", null, chapterId,
                    isRoster: true, rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: new FixedRosterTitleItem[0], 
                    rosterTitleQuestionId: null));

        It should_not_fail = () =>
            exception.ShouldEqual(null);

        private static Exception exception;
        private static Questionnaire questionnaire;

        private static Guid chapterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid groupId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid responsibleId = Guid.Parse("CCCCCCCCCCCCCCCCDDDDDDDDDDDDDDDD");
        private static Guid rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");
    }
}