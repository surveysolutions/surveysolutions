using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests.AssignmentTests
{
    public class when_query_for_assignment_from_synchronization_service
    {
        private AssignmentApiDocument assignment;
        private AssignmentApiDocument assignmentResponse;

        private OnlineSynchronizationService synchronizationService;

        [OneTimeSetUp]
        public async Task Context()
        {
            this.assignment = Create.Entity
                .AssignmentApiDocument(1, 5, Create.Entity.QuestionnaireIdentity(Id.g1))
                .WithAnswer(Create.Entity.Identity(Guid.NewGuid()), "123", "serializeddata")
                .WithAnswer(Create.Entity.Identity(Guid.NewGuid()), "456", "serializeddata2")
                .Build();

            var restService = Mock.Of<IRestService>(
                x => x.GetAsync<AssignmentApiDocument>(It.IsAny<string>(), null, null, It.IsAny<RestCredentials>(),
                         It.IsAny<CancellationToken>()) == Task.FromResult( this.assignment));

            this.synchronizationService = Create.Service.SynchronizationService(restService: restService);

            await this.Act();
        }

        public async Task Act() => this.assignmentResponse = await this.synchronizationService.GetAssignmentAsync(1, default(CancellationToken));

        [Test]
        public void should_be_able_deserialize_identifying_data()
        {
            Assert.That(this.assignmentResponse.Answers, Has.All.Matches<AssignmentApiDocument.InterviewSerializedAnswer>(f =>
                this.assignment.Answers.Any(e =>
                    f.Identity == e.Identity &&
                    f.SerializedAnswer == e.SerializedAnswer)));
        }

        [Test]
        public void should_be_able_to_deserialize_quantity() => Assert.That(this.assignmentResponse.Quantity, Is.EqualTo(this.assignment.Quantity));

        [Test]
        public void should_be_able_to_deserialize_id() => Assert.That(this.assignmentResponse.Id, Is.EqualTo(this.assignment.Id));

        [Test]
        public void should_be_able_to_deserialize_questionnaireId() => Assert.That(this.assignmentResponse.QuestionnaireId, Is.EqualTo(this.assignment.QuestionnaireId));
    }
}
