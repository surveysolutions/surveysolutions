﻿using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_updating_non_roster_group_that_contains_prefilled_question : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            var chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var prefilledQuestionId = Guid.Parse("22222222222222222222222222222222");

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(Create.Event.NumericQuestionAdded(
                publicKey: rosterSizeQuestionId, isInteger:true, groupPublicKey: chapterId));
            
            questionnaire.Apply(new NewGroupAdded { PublicKey = rosterId });
            questionnaire.Apply(Create.Event.NumericQuestionAdded(
                publicKey: prefilledQuestionId, isInteger: true, groupPublicKey: rosterId, featured:true));
            
            eventContext = new EventContext();
        };

        Because of = () =>
            exception =
                Catch.Exception(
                    () =>
                        questionnaire.UpdateGroup(rosterId, responsibleId, "title",null, rosterSizeQuestionId, null, null, hideIfDisabled: false, isRoster: true,
                            rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null));

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__missing__ = () =>
            exception.Message.ToLower().ShouldContain("become");

        It should_throw_exception_with_message_containting__roster__ = () =>
            exception.Message.ToLower().ShouldContain("roster");

        It should_throw_exception_with_message_containting__prefilled__ = () =>
            exception.Message.ToLower().ShouldContain("pre-filled");

        It should_throw_exception_with_message_containting__question__ = () =>
            exception.Message.ToLower().ShouldContain("question");


        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static Exception exception;
    }
}