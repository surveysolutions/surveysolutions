using System;
using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.FileSystem;


namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireAssemblyFileAccessorTests
{
    internal class when_getting_assembly_as_byte_array : QuestionnaireAssemblyFileAccessorTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            AssemblyServiceMock.Setup(x => x.GetAssemblyInfo(Moq.It.IsAny<string>())).Returns(new AssemblyInfo() { Content = data1 });
            questionnaireAssemblyFileAccessor = CreateQuestionnaireAssemblyFileAccessor(assemblyService: AssemblyServiceMock.Object);
            BecauseOf();
        }

        public void BecauseOf() => result = questionnaireAssemblyFileAccessor.GetAssemblyAsByteArray(questionnaireId, version);

        [NUnit.Framework.Test] public void should_data_of_returned_file_be_equal_to_data1 () =>
            result.Should().BeEquivalentTo(data1);

        private static QuestionnaireAssemblyAccessor questionnaireAssemblyFileAccessor;
        private static readonly Mock<IAssemblyService> AssemblyServiceMock = CreateIAssemblyService();
        private static Guid questionnaireId = Guid.Parse("33332222111100000000111122223333");
        private static long version = 3;

        private static byte[] data1 = new byte[] { 1 };

        private static byte[] result;
    }
}
