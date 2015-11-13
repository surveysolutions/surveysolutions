﻿using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests.CascadigOptions
{
    internal class when_updating_question_setting_to_cascade_from_non_existing_question : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            rootGroupId = Guid.NewGuid();
            actorId = Guid.NewGuid();
            questionnaire = CreateQuestionnaireWithOneGroup(actorId, groupId: rootGroupId);

            parentQuestionId = Guid.NewGuid();
            updatedQuestionId = Guid.NewGuid();

            questionnaire.Apply(CreateNewQuestionAdded
            (
                publicKey : parentQuestionId,
                groupPublicKey:rootGroupId,
                questionType : QuestionType.SingleOption
            ));
            questionnaire.Apply(CreateNewQuestionAdded
            (
                publicKey: updatedQuestionId,
                groupPublicKey: rootGroupId,
                questionType: QuestionType.SingleOption
            ));
        };

        Because of = () => exception = Catch.Exception(() => questionnaire.UpdateSingleOptionQuestion(updatedQuestionId, 
            "title",
            "varia",
            null,
            false,
            QuestionScope.Interviewer,
            null,
            null,
            null,
            null,
            actorId,
            new Option[]{}, 
            null,
            false,
            cascadeFromQuestionId: Guid.NewGuid()));

        It should_not_allow_cascades_from_non_existing_question = () =>
        {
            var ex = exception as QuestionnaireException;
            ex.ShouldNotBeNull();
            new[] { "cascade", "should", "exist" }.ShouldEachConformTo(keyword => ex.Message.ToLower().Contains(keyword));
        };
        private static Guid rootGroupId;
        private static Questionnaire questionnaire;
        private static Guid parentQuestionId;
        private static Guid updatedQuestionId;
        private static Exception exception;
        private static Guid actorId;
    }
}

