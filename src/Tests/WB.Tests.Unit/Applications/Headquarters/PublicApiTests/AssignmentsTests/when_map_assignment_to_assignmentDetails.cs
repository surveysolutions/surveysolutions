using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.PublicApi.Models;
using It = Moq.It;

namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests.AssignmentsTests
{
    public class when_map_assignment_to_assignmentDetails : AssignmentsPublicApiMapProfileSpecification
    {
        protected Assignment Assignment { get; set; }
        protected AssignmentDetails AssignmentDetails { get; set; }

        public override void Context()
        {
            this.Assignment = Create.Entity.Assignment(1, Create.Entity.QuestionnaireIdentity(Id.g1, 10),
                responsibleName: "TestName", assigneeSupervisorId: Id.gE,
                interviewSummary: new HashSet<InterviewSummary>{
                    new InterviewSummary(),
                    new InterviewSummary()
                });

            this.Assignment.SetIdentifyingData(new List<IdentifyingAnswer>
            {
                Create.Entity.IdentifyingAnswer(this.Assignment, answer: "Test22", identity: Create.Identity(Id.g2)),
                Create.Entity.IdentifyingAnswer(this.Assignment, answer: "Test33", identity: Create.Identity(Id.g3))
            });
        }

        public override void Because()
        {
            this.AssignmentDetails = this.mapper.Map<AssignmentDetails>(this.Assignment);
        }

        [Test]
        public void should_map_id() => Assert.That(this.AssignmentDetails.Id, Is.EqualTo(this.Assignment.Id));

        [Test]
        public void should_map_responsible() =>
            Assert.That(this.AssignmentDetails.ResponsibleId, Is.EqualTo(this.Assignment.ResponsibleId));

        [Test]
        public void should_map_quantity() =>
            Assert.That(this.AssignmentDetails.Quantity, Is.EqualTo(this.Assignment.Quantity));

        [Test]
        public void should_map_CreatedAt() =>
            Assert.That(this.AssignmentDetails.CreatedAtUtc, Is.EqualTo(this.Assignment.CreatedAtUtc));

        [Test]
        public void should_map_UpdatedAt() =>
            Assert.That(this.AssignmentDetails.UpdatedAtUtc, Is.EqualTo(this.Assignment.UpdatedAtUtc));

        [Test]
        public void should_map_Archived() =>
            Assert.That(this.AssignmentDetails.Archived, Is.EqualTo(this.Assignment.Archived));

        [Test]
        public void should_map_QuestionnaireId() =>
            Assert.That(this.AssignmentDetails.QuestionnaireId, Is.EqualTo(this.Assignment.QuestionnaireId.ToString()));

        [Test]
        public void should_map_InterviewsCount() =>
            Assert.That(this.AssignmentDetails.InterviewsCount, Is.EqualTo(this.Assignment.InterviewSummaries.Count));

        [Test]
        public void should_map_ResponsibleName() =>
            Assert.That(this.AssignmentDetails.ResponsibleName, Is.EqualTo(this.Assignment.Responsible.Name));

        [Test]
        public void should_map_IdentifyingAnswer_Answer() =>
            Assert.That(this.AssignmentDetails.IdentifyingData[0].Answer, Is.EqualTo(this.Assignment.IdentifyingData[0].Answer));

        [Test]
        public void should_map_IdentifyingAnswer_QuestionId() =>
            Assert.That(this.AssignmentDetails.IdentifyingData[0].Identity, Is.EqualTo(this.Assignment.IdentifyingData[0].Identity.ToString()));

        [Test]
        public void should_map_IdentifyingAnswer_Variable_name_from_questionnaire() =>
            Assert.That(this.AssignmentDetails.IdentifyingData[0].Variable, Is.EqualTo("test2"));
        
        [Test]
        public void should_query_questionnaire_storage() =>
            this.storageMock.Verify(x => x.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()), Times.Once);
    }
}