using System;
using FluentAssertions;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.FileSystem;


namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireAssemblyFileAccessorTests
{
    internal class when_storing_assembly_with_empty_content : QuestionnaireAssemblyFileAccessorTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            AssemblyServiceMock.Setup(x => x.GetAssemblyInfo(Moq.It.IsAny<string>())).Returns(new AssemblyInfo() {  });
            questionnaireAssemblyFileAccessor = CreateQuestionnaireAssemblyFileAccessor(assemblyService: AssemblyServiceMock.Object);
            BecauseOf();
        }

        public void BecauseOf() =>
            exception = Catch.Only<ArgumentException>(() => questionnaireAssemblyFileAccessor.StoreAssembly(questionnaireId, version, data1));

        [NUnit.Framework.Test] public void should_not_exception_be_null () =>
            exception.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_throw_ArgumentException () =>
            exception.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__dont_have_permissions__ () =>
            new[] { "assembly", "empty", version.ToString() }.ShouldEachConformTo(keyword => exception.Message.ToLower().Contains(keyword));


        private static QuestionnaireAssemblyAccessor questionnaireAssemblyFileAccessor;
        private static readonly Mock<IAssemblyService> AssemblyServiceMock = CreateIAssemblyService();
        private static Guid questionnaireId = Guid.Parse("33332222111100000000111122223333");
        private static long version = 3;

        private static ArgumentException exception;

        private static string data1 = string.Empty;
        

    }
}
