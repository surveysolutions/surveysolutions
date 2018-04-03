using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.InterviewerQuestionnaireAccessorTests
{
    internal class when_storing_questionnaire_assembly : InterviewerQuestionnaireAccessorTestsContext
    {
        [Test]
        public async Task should_store_questionnaire_assembly_to_plain_storage()
        {
            byte[] bytesOfQuestionnaireAssembly = new byte[0];
            QuestionnaireIdentity questionnaireIdentity = Create.Entity.QuestionnaireIdentity();
            Mock<IQuestionnaireAssemblyAccessor> mockOfQuestionnaireAssemblyFileAccessor = new Mock<IQuestionnaireAssemblyAccessor>();

            var interviewerQuestionnaireAccessor = CreateInterviewerQuestionnaireAccessor(
                questionnaireAssemblyFileAccessor: mockOfQuestionnaireAssemblyFileAccessor.Object);

            // Act
            await interviewerQuestionnaireAccessor.StoreQuestionnaireAssemblyAsync(questionnaireIdentity, bytesOfQuestionnaireAssembly);

            // Assert
            mockOfQuestionnaireAssemblyFileAccessor.Verify(x => x.StoreAssemblyAsync(questionnaireIdentity, bytesOfQuestionnaireAssembly), Times.Once);
        }
    }
}
