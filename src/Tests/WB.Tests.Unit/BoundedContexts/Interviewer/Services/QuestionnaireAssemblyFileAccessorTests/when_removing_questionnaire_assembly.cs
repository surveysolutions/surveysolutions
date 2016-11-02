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
                fileSystemAccessor: mockOfFileSystemAccessor.Object);
        };

        Because of = () =>
            interviewerQuestionnaireAssemblyFileAccessor.RemoveAssembly(questionnaireIdentity);

        It should_call_delete_file_of_async_file_system_accessor = () =>
            mockOfFileSystemAccessor.Verify(x => x.DeleteFile(Moq.It.IsAny<string>()), Times.Once);

        private static readonly QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("11111111111111111111111111111111"), 1);
        private static readonly Mock<IFileSystemAccessor> mockOfFileSystemAccessor = new Mock<IFileSystemAccessor>();
        private static InterviewerQuestionnaireAssemblyFileAccessor interviewerQuestionnaireAssemblyFileAccessor;
    }
}
