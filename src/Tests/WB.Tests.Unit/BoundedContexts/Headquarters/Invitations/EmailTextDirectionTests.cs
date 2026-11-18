using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Invitations;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Invitations
{
    [TestOf(typeof(EmailTextDirection))]
    public class EmailTextDirectionTests
    {
        [TestCase("دعوة للمشاركة", "مرحبا", true)]
        [TestCase("%SURVEYNAME%", "مرحبا بكم في %SURVEYNAME%", true)]
        [TestCase("%SURVEYNAME% - دعوة", "Hello", true)]
        [TestCase("2026: دعوة", "", true)]
        [TestCase("Invitation", "مرحبا", false)]
        [TestCase("", "<b>Hello</b> مرحبا", false)]
        [TestCase("", "&lt;مرحبا&gt;", true)]
        [TestCase("%SURVEYNAME%", "%SURVEYLINK%", false)]
        public void when_detecting_direction_should_use_first_letter_of_authored_text(string subject, string text, bool isRightToLeft)
        {
            var template = new WebInterviewEmailTemplate(subject, text, null, null);

            Assert.That(EmailTextDirection.IsRightToLeft(template), Is.EqualTo(isRightToLeft));
        }

        [Test]
        public void when_template_has_only_button_text_in_arabic_should_be_right_to_left()
        {
            var template = new WebInterviewEmailTemplate("%QUESTIONNAIRE%", "%SURVEYLINK%", null, "ابدأ");

            Assert.That(EmailTextDirection.IsRightToLeft(template), Is.True);
        }

        [Test]
        public void when_creating_content_with_english_title_in_arabic_template_should_keep_right_to_left()
        {
            var template = new WebInterviewEmailTemplate("%SURVEYNAME%", "مرحبا %SURVEYLINK%", "كلمة المرور", "ابدأ");

            var content = new EmailContent(template, "Household survey", "https://example.org/start", "secret");

            Assert.That(content.Subject, Is.EqualTo("Household survey"));
            Assert.That(content.IsRightToLeft, Is.True);
        }

        [Test]
        public void when_marking_plain_text_should_prefix_only_non_empty_lines()
        {
            var marked = EmailTextDirection.MarkRightToLeftLines("مرحبا\n\n  \nhttps://example.org");

            Assert.That(marked, Is.EqualTo("\u200Fمرحبا\n\n  \n\u200Fhttps://example.org"));
        }
    }
}
