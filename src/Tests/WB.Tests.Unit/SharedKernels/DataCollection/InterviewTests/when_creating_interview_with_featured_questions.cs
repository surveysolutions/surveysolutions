﻿using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Moq;
using NSubstitute;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_creating_interview_with_two_featured_questions : InterviewTestsContext
    {
        private Establish context = () =>
        {
            questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            responsibleSupervisorId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAA00");
            questionnaireVersion = 18;
            var prefilledQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var prefilledQuestion1Id = Guid.Parse("ACDC1CCCCCCCCCCCCCCCCCCCCCCCCCCC");
            
            var answersToFeaturedQuestions = new List<InterviewAnswer>
            {
                Create.Entity.InterviewAnswer(Create.Identity(prefilledQuestionId), TextAnswer.FromString("answer")),
                Create.Entity.InterviewAnswer(Create.Identity(prefilledQuestion1Id), TextAnswer.FromString("answer 1"))
            };

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextQuestion(questionId: prefilledQuestionId),
                Create.Entity.TextQuestion(questionId: prefilledQuestion1Id),
            }));

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);
            
            expressionState = Substitute.For<ILatestInterviewExpressionState>();
            expressionState.Clone().Returns(expressionState);
            var structuralChanges = new StructuralChanges();
            expressionState.GetStructuralChanges().Returns(structuralChanges);
            expressionState.ProcessValidationExpressions().Returns(new ValidityChanges( new List<Identity>(), new List<Identity>())); 
                        
            var interviewExpressionStatePrototypeProvider = Mock.Of<IInterviewExpressionStatePrototypeProvider>(_ =>
                _.GetExpressionState(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>()) == expressionState);

            command = Create.Command.CreateInterview(questionnaireId, questionnaireVersion, responsibleSupervisorId,
                answersToFeaturedQuestions, userId);
            interview = Create.AggregateRoot.Interview(questionnaireRepository: questionnaireRepository,
                expressionProcessorStatePrototypeProvider: interviewExpressionStatePrototypeProvider);
        };

        Because of = () =>
            interview.CreateInterview(command);

        It should_call_ProcessValidationExpressions_once = () =>
            expressionState.Received(1).ProcessValidationExpressions();

        private static Guid questionnaireId;
        private static long questionnaireVersion;
        private static Guid userId;
        private static Guid responsibleSupervisorId;
        private static Interview interview;
        private static ILatestInterviewExpressionState expressionState;
        private static CreateInterview command;
    }
}