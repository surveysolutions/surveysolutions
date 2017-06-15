﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests.AssignmentTests
{
    public class when_query_for_assignments_from_synchronization_service
    {
        private AssignmentApiView assignment;
        private List<AssignmentApiView> assignments;
        private SynchronizationService synchronizationService;

        [OneTimeSetUp]
        public async Task Context()
        {
            this.assignment = Create.Entity
                .AssignmentApiView(1, 5, 1, Create.Entity.QuestionnaireIdentity(Id.g1))
                .WithAnswer(Create.Entity.Identity(Guid.NewGuid()), "123")
                .WithAnswer(Create.Entity.Identity(Guid.NewGuid()), "456")
                .Build();

            var restService = Mock.Of<IRestService>(
                x => x.GetAsync<List<AssignmentApiView>>(It.IsAny<string>(), null, null, It.IsAny<RestCredentials>(),
                         It.IsAny<CancellationToken>()) == Task.FromResult(new List<AssignmentApiView> { this.assignment }));

            synchronizationService = Create.Service.SynchronizationService(restService: restService);

            await Act();
        }

        public async Task Act() => this.assignments = await synchronizationService.GetAssignmentsAsync(default(CancellationToken));

        [Test]
        public void should_be_able_deserialize_identifying_data()
        {
            var assignmentDocument = assignments.Single();
            
            assignmentDocument.Answers.SequenceEqual(assignment.Answers, source => source.Identity, target => target.Identity);
            assignmentDocument.Answers.SequenceEqual(assignment.Answers, source => source.AnswerAsString, target => target.AnswerAsString);
        }

        [Test]
        public void should_be_able_to_deserialize_quantity() => Assert.That(assignments.Single().Quantity, Is.EqualTo(assignment.Quantity));

        [Test]
        public void should_be_able_to_deserialize_id() => Assert.That(assignments.Single().Id, Is.EqualTo(assignment.Id));

        [Test]
        public void should_be_able_to_deserialize_questionnaireId() => Assert.That(assignments.Single().QuestionnaireId, Is.EqualTo(assignment.QuestionnaireId));
    }
}