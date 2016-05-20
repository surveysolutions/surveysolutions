﻿using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using NHibernate.Util;
using NSubstitute;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.V6;
using WB.Core.SharedKernels.DataCollection.V7;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Validation
{
    internal class when_answering_question_with_multiple_failed_validation_conditions : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionId = Guid.NewGuid();
            questionIdentity = Create.Other.Identity(questionId, RosterVector.Empty);

            var interviewExpressionStateV7 = Substitute.For<ILatestInterviewExpressionState>();
            IDictionary<Identity, IReadOnlyList<FailedValidationCondition>> failedValidatoinConditions = new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>();
            failedValidatoinConditions.Add(questionIdentity, new List<FailedValidationCondition>
            {
                new FailedValidationCondition() {FailedConditionIndex = 0},
                new FailedValidationCondition() {FailedConditionIndex = 1}
            });

            var answersDeclaredInvalid = new List<Identity> {questionIdentity};
            interviewExpressionStateV7.ProcessValidationExpressions()
                                      .Returns(new ValidityChanges(new List<Identity>(), answersDeclaredInvalid, failedValidatoinConditions));
            interviewExpressionStateV7.ProcessEnablementConditions()
                                      .ReturnsForAnyArgs(Create.Other.EnablementChanges());

            interviewExpressionStateV7.Clone().Returns(interviewExpressionStateV7);

            IInterviewExpressionStatePrototypeProvider expressionProcessorProvider = Substitute.For<IInterviewExpressionStatePrototypeProvider>();
            expressionProcessorProvider.GetExpressionState(new Guid(), 1)
                                       .ReturnsForAnyArgs(interviewExpressionStateV7);

            var questionnaire = CreateQuestionnaireDocumentWithOneChapter(Create.Other.Question(questionId: questionId,
                questionType: QuestionType.Text,
                validationConditions: new List<ValidationCondition> {
                    new ValidationCondition("validation1", "message1"),
                    new ValidationCondition("validation2", "message2")
                }));

            IPlainQuestionnaireRepository questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaire.PublicKey, Create.Other.PlainQuestionnaire(questionnaire));
            interview = CreateInterview(expressionProcessorStatePrototypeProvider: expressionProcessorProvider,
                questionnaireId: questionnaire.PublicKey,
                questionnaireRepository: questionnaireRepository);
            eventContext = new EventContext();
        };

        Because of = () => interview.AnswerTextQuestion(Guid.NewGuid(), questionIdentity.Id, questionIdentity.RosterVector, DateTime.UtcNow, "answer");

        It should_raise_events_with_mutliple_validations_info = () => 
            eventContext.ShouldContainEvent<AnswersDeclaredInvalid>(e => e.Questions.First().Equals(questionIdentity) && e.FailedValidationConditions.Count == 1 && e.FailedValidationConditions[questionIdentity].Count == 2);

        static Interview interview;
        static Identity questionIdentity;
        static EventContext eventContext;
    }
}