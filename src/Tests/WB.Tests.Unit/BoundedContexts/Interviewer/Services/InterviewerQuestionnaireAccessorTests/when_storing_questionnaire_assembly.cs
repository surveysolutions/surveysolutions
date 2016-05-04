using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.InterviewerQuestionnaireAccessorTests
{
    internal class when_storing_questionnaire_assembly : InterviewerQuestionnaireAccessorTestsContext
    {
        Establish context = () =>
        {
            interviewerQuestionnaireAccessor = CreateInterviewerQuestionnaireAccessor(
                questionnaireAssemblyFileAccessor: mockOfQuestionnaireAssemblyFileAccessor.Object);
        };

        Because of = async () =>
            await interviewerQuestionnaireAccessor.StoreQuestionnaireAssemblyAsync(questionnaireIdentity, bytesOfQuestionnaireAssembly);

        It should_store_questionnaire_assembly_to_plain_storage = () =>
            mockOfQuestionnaireAssemblyFileAccessor.Verify(x => x.StoreAssemblyAsync(questionnaireIdentity, bytesOfQuestionnaireAssembly), Times.Once);

        private static readonly byte[] bytesOfQuestionnaireAssembly = new byte[0];
        private static readonly QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("11111111111111111111111111111111"), 1);
        private static readonly Mock<IQuestionnaireAssemblyFileAccessor> mockOfQuestionnaireAssemblyFileAccessor = new Mock<IQuestionnaireAssemblyFileAccessor>();
        private static InterviewerQuestionnaireAccessor interviewerQuestionnaireAccessor;
    }
}
