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

            var repositoryWithoutQuestionnaire = Mock.Of<IQuestionnaireStorage>();

            this.command = Create.Command.CreateInterview(this.questionnaireId, 1, this.responsibleSupervisorId,
                new List<InterviewAnswer>(), userId: this.userId);
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
        private Interview interview;
        private CreateInterview command;
        private Action act;
    }
}
