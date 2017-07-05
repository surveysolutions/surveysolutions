using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    [TestFixture]
    internal class when_creating_interview_and_questionnaire_does_not_exist : InterviewTestsContext
    {
        [OneTimeSetUp]
        public void context()
        {
            questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            responsibleSupervisorId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAA00");
            answersToFeaturedQuestions = new Dictionary<Guid, AbstractAnswer>();

            var repositoryWithoutQuestionnaire = Mock.Of<IQuestionnaireStorage>();

            command = Create.Command.CreateInterviewCommand(questionnaireId, 1, responsibleSupervisorId,
                answersToFeaturedQuestions, userId: userId);
           interview = Create.AggregateRoot.Interview(questionnaireRepository: repositoryWithoutQuestionnaire);

           // Act
           act =() => interview.CreateInterview(command);
        }
         

        [Test]
        public void should_throw_interview_exception() =>
            this.act.ShouldThrow<InterviewException>();

        private Guid questionnaireId;
        private Guid userId;
        private Guid responsibleSupervisorId;
        private Dictionary<Guid, AbstractAnswer> answersToFeaturedQuestions;
        private Interview interview;
        private CreateInterviewCommand command;
        private Action act;
    }
}
