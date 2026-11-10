using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Views;

namespace WB.Tests.Unit.Designer.Services
{
    [TestFixture]
    [TestOf(typeof(GlobalNoticeService))]
    public class GlobalNoticeServiceTests
    {
        [Test]
        public void when_notice_is_not_stored_should_return_null()
        {
            var service = new GlobalNoticeService(new TestPlainStorage<GlobalNotice>());

            service.GetMessage().Should().BeNull();
        }

        [Test]
        public void when_setting_message_should_store_trimmed_message()
        {
            var storage = new TestPlainStorage<GlobalNotice>();
            var service = new GlobalNoticeService(storage);

            service.SetMessage("  Maintenance tonight  ");

            service.GetMessage().Should().Be("Maintenance tonight");
            storage.GetById(AppSetting.GlobalNoticeKey)!.Message.Should().Be("Maintenance tonight");
        }

        [Test]
        public void when_setting_too_long_message_should_truncate_it_to_max_length()
        {
            var service = new GlobalNoticeService(new TestPlainStorage<GlobalNotice>());

            service.SetMessage(new string('a', GlobalNoticeService.MaxMessageLength + 10));

            service.GetMessage().Should().HaveLength(GlobalNoticeService.MaxMessageLength);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void when_setting_empty_message_should_remove_stored_notice(string message)
        {
            var storage = new TestPlainStorage<GlobalNotice>();
            storage.Store(new GlobalNotice { Message = "old" }, AppSetting.GlobalNoticeKey);
            var service = new GlobalNoticeService(storage);

            service.SetMessage(message);

            storage.GetById(AppSetting.GlobalNoticeKey).Should().BeNull();
            service.GetMessage().Should().BeNull();
        }

        [Test]
        public void when_stored_message_is_whitespace_should_return_null()
        {
            var storage = new TestPlainStorage<GlobalNotice>();
            storage.Store(new GlobalNotice { Message = "  " }, AppSetting.GlobalNoticeKey);

            new GlobalNoticeService(storage).GetMessage().Should().BeNull();
        }
    }
}
