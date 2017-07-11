using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewSynchronizationDtoFactoryTests
{
    internal class when_build_for_interview_with_key_and_assignment_id : InterviewSynchronizationDtoFactoryTestContext
    {
        [Test]
        public void should_put_data_to_dto()
        {
            var interviewKey = Create.Entity.InterviewKey(499);
            var assignmentId = 55;

            var interviewData = CreateInterviewData();
            interviewData.InterviewKey = interviewKey.ToString();
            interviewData.AssignmentId = assignmentId;

            var interviewSynchronizationDtoFactory = CreateInterviewSynchronizationDtoFactory(CreateQuestionnaireDocumentWithOneChapter());

            // Act
            InterviewSynchronizationDto result = interviewSynchronizationDtoFactory.BuildFrom(interviewData, "comment", null, null);

            // Assert
            Assert.That(result.InterviewKey, Is.EqualTo(interviewKey));
            Assert.That(interviewData.AssignmentId, Is.EqualTo(assignmentId));
        }
    }
}