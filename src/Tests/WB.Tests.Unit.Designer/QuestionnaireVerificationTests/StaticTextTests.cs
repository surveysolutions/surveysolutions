using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    [TestFixture]
    public class StaticTextTests
    {
        [Test]
        public void static_text_with_text()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.StaticText(text: "static text"),
                })
                .ExpectNoError("WB0071");

        [Test]
        public void static_text_with_attachment()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.StaticText(attachmentName: "attachemnt#1"),
                })
                .ExpectNoError("WB0071");

        [Test]
        public void static_text_without_text_and_attachment()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.StaticText(text: ""),
                })
                .ExpectError("WB0071");

        [Test]
        public void when_non_string_variable_referenced_Should_return_WB0390_error()
        {
            Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.StaticText(attachmentName: "var1"),
                Create.Variable(variableName: "var1", type: VariableType.Boolean)
            }).ExpectError("WB0390");
        }
    }
}
