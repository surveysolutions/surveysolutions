﻿using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireTests
{
    internal class when_questionnaire_has_multi_question_and_its_type_does_not_support_GetMaxSelectedAnswerOptions_method
        : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            QuestionnaireDocument questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(new IComposite[]
            {
                new MultyOptionsQuestion()
                {
                    PublicKey = validatedQuestionId,
                    MaxAllowedAnswers = proposedSelectedAnswerOptions,
                    QuestionType = QuestionType.SingleOption
                }
            });

            questionnaire = CreateQuestionnaire(Guid.NewGuid(), questionnaireDocument);
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.GetQuestionnaire().GetMaxSelectedAnswerOptions(validatedQuestionId));

        It should_throw_questionnaire_exception = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containing__custom_validation__ = () =>
            exception.Message.ShouldContain("Cannot return maximum for selected answers");

        private static int? proposedSelectedAnswerOptions = 5;
        private static Exception exception;
        private static Questionnaire questionnaire;
        private static Guid validatedQuestionId = new Guid("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
    }
}