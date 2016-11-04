using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.Infrastructure.FileSystem;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireAssemblyFileAccessorTests
{
    internal class when_storing_assembly_with_empty_content : QuestionnaireAssemblyFileAccessorTestsContext
    {
        Establish context = () =>
        {
            questionnaireAssemblyFileAccessor = CreateQuestionnaireAssemblyFileAccessor(fileSystemAccessor: FileSystemAccessorMock.Object);
        };

        Because of = () =>
            exception = Catch.Only<ArgumentException>(() => questionnaireAssemblyFileAccessor.StoreAssembly(questionnaireId, version, data1));

        It should_not_exception_be_null = () =>
            exception.ShouldNotBeNull();

        It should_throw_ArgumentException = () =>
            exception.ShouldNotBeNull();

        It should_throw_exception_with_message_containting__dont_have_permissions__ = () =>
            new[] { "assembly", "empty", version.ToString() }.ShouldEachConformTo(keyword => exception.Message.ToLower().Contains(keyword));


        private static QuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor;
        private static readonly Mock<IFileSystemAccessor> FileSystemAccessorMock = CreateIFileSystemAccessorMock();
        private static Guid questionnaireId = Guid.Parse("33332222111100000000111122223333");
        private static long version = 3;

        private static ArgumentException exception;

        private static string data1 = string.Empty;
        

    }
}
