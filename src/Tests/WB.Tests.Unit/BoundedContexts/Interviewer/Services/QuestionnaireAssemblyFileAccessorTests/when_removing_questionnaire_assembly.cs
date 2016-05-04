using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.QuestionnaireAssemblyFileAccessorTests
{
    internal class when_removing_questionnaire_assembly : QuestionnaireAssemblyFileAccessorTestsContext
    {
        Establish context = () =>
        {
            interviewerQuestionnaireAssemblyFileAccessor = CreateQuestionnaireAssemblyFileAccessor(
                asyncFileSystemAccessor: mockOfAsyncFileSystemAccessor.Object);
        };

        Because of = async () =>
            await interviewerQuestionnaireAssemblyFileAccessor.RemoveAssemblyAsync(questionnaireIdentity);

        It should_call_delete_file_of_async_file_system_accessor = () =>
            mockOfAsyncFileSystemAccessor.Verify(x => x.DeleteFileAsync(Moq.It.IsAny<string>()), Times.Once);

        private static readonly QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("11111111111111111111111111111111"), 1);
        private static readonly Mock<IAsynchronousFileSystemAccessor> mockOfAsyncFileSystemAccessor = new Mock<IAsynchronousFileSystemAccessor>();
        private static InterviewerQuestionnaireAssemblyFileAccessor interviewerQuestionnaireAssemblyFileAccessor;
    }
}
