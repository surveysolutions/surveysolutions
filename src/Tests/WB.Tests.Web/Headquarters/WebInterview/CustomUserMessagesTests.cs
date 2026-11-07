using System;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.UI.Headquarters.Resources;
using WB.UI.Headquarters.Services.Impl;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.WebInterview
{
    public class CustomUserMessagesTests
    {
        [Test]
        [SetUICulture("en-US")]
        public void each_enum_member_should_have_translated_name()
        {
            foreach (var enumMember in Enum.GetValues(typeof(WebInterviewUserMessages)))
            {
                var val = (WebInterviewUserMessages) enumMember;

                Assert.That(val.ToUiString(), Is.Not.Null, $"{typeof(EnumNames)} should contain a translation for member {val}");

                var description =
                    WebInterviewSetup.ResourceManager.GetString($"{nameof(WebInterviewUserMessages)}_{val}_Descr");

                Assert.That(description, Is.Not.Null.Or.Empty, 
                    $"{typeof(WebInterviewSetup)} should contain description for {val}: {nameof(WebInterviewUserMessages)}_{val}_Descr");
            }
           
        }

        [Test]
        public void should_have_default_for_every_message_and_empty_complete_screen_text()
        {
            foreach (WebInterviewUserMessages message in Enum.GetValues(typeof(WebInterviewUserMessages)))
            {
                Assert.That(WebInterviewConfig.DefaultMessages.ContainsKey(message), Is.True, $"Default is missing for {message}");
            }

            Assert.That(WebInterviewConfig.DefaultMessages[WebInterviewUserMessages.CompleteScreenText], Is.Empty);
        }
    }
}
