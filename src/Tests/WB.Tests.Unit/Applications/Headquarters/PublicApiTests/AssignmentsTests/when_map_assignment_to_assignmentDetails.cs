using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
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
                responsibleName: "TestName",
                interviewSummary: new HashSet<InterviewSummary>{
                    new InterviewSummary(),
                    new InterviewSummary()
                });

            this.Assignment.SetAnswers(new List<IdentifyingAnswer>
            {
                new IdentifyingAnswer(this.Assignment)
                {
                    Answer = "Test22",
                    QuestionId = Id.g2
                },
                new IdentifyingAnswer(this.Assignment)
                {
                    Answer = "Test33",
                    QuestionId = Id.g3
                }
            });
        }

        public override void Because()
        {
            this.AssignmentDetails = this.mapper.Map<AssignmentDetails>(this.Assignment);
        }

        [Test]
        public void should_map_id() => this.AssignmentDetails.Id.ShouldEqual(this.Assignment.Id);

        [Test]
        public void should_map_responsible() => 
            this.AssignmentDetails.ResponsibleId.ShouldEqual(this.Assignment.ResponsibleId);

        [Test]
        public void should_map_capacity() =>
            this.AssignmentDetails.Capacity.ShouldEqual(this.Assignment.Capacity);

        [Test]
        public void should_map_CreatedAt() =>
            this.AssignmentDetails.CreatedAtUtc.ShouldEqual(this.Assignment.CreatedAtUtc);

        [Test]
        public void should_map_UpdatedAt() =>
            this.AssignmentDetails.UpdatedAtUtc.ShouldEqual(this.Assignment.UpdatedAtUtc);

        [Test]
        public void should_map_Archived() =>
            this.AssignmentDetails.Archived.ShouldEqual(this.Assignment.Archived);

        [Test]
        public void should_map_QuestionnaireId() =>
            this.AssignmentDetails.QuestionnaireId.ShouldEqual(this.Assignment.QuestionnaireId.ToString());

        [Test]
        public void should_map_InterviewsCount() =>
            this.AssignmentDetails.InterviewsCount.ShouldEqual(this.Assignment.InterviewSummaries.Count);

        [Test]
        public void should_map_ResponsibleName() =>
            this.AssignmentDetails.ResponsibleName.ShouldEqual(this.Assignment.Responsible.Name);

        [Test]
        public void should_map_IdentifyingAnswer_Answer() =>
            this.AssignmentDetails.IdentifyingData[0].Answer.ShouldEqual(this.Assignment.IdentifyingData[0].Answer);

        [Test]
        public void should_map_IdentifyingAnswer_QuestionId() =>
            this.AssignmentDetails.IdentifyingData[0].QuestionId.ShouldEqual(this.Assignment.IdentifyingData[0].QuestionId);

        [Test]
        public void should_map_IdentifyingAnswer_Variable_name_from_questionnaire() =>
            this.AssignmentDetails.IdentifyingData[0].Variable.ShouldEqual("test2");

        [Test]
        public void should_query_questionnaire_storage() =>
            this.storageMock.Verify(x => x.GetQuestionnaireDocument(It.IsAny<Guid>(), It.IsAny<long>()), Times.Once);
    }
}