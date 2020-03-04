using NUnit.Framework;

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
    }
}
