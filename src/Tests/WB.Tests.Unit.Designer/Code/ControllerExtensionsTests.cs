using System;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using NUnit.Framework;
using WB.UI.Designer.Extensions;
using WB.UI.Shared.Web.Extensions;

namespace WB.Tests.Unit.Designer.Code
{
    [TestFixture]
    [TestOf(typeof(ControllerExtensions))]
    public class ControllerExtensionsTests
    {
        private class TestController : Controller { }

        private static Controller CreateController()
            => new TestController
            {
                TempData = new TempDataDictionary(new Microsoft.AspNetCore.Http.DefaultHttpContext(), Mock.Of<ITempDataProvider>())
            };

        [Test]
        public void Error_should_write_message_to_error_alert_key()
        {
            var controller = CreateController();

            controller.Error("boom");

            controller.TempData[Alerts.ERROR].Should().Be("boom");
        }

        [Test]
        public void Success_should_write_message_to_success_alert_key()
        {
            var controller = CreateController();

            controller.Success("ok");

            controller.TempData[Alerts.SUCCESS].Should().Be("ok");
        }

        [Test]
        public void Error_when_key_exists_and_not_append_should_overwrite()
        {
            var controller = CreateController();
            controller.Error("first");

            controller.Error("second");

            controller.TempData[Alerts.ERROR].Should().Be("second");
        }

        [Test]
        public void Error_when_key_exists_and_append_should_concatenate_with_new_line()
        {
            var controller = CreateController();
            controller.Error("first");

            controller.Error("second", append: true);

            controller.TempData[Alerts.ERROR].Should().Be("first" + Environment.NewLine + "second");
        }

        [Test]
        public void Error_and_Success_should_not_interfere()
        {
            var controller = CreateController();

            controller.Error("e");
            controller.Success("s");

            controller.TempData[Alerts.ERROR].Should().Be("e");
            controller.TempData[Alerts.SUCCESS].Should().Be("s");
        }
    }

    [TestFixture]
    [TestOf(typeof(EnvironmentExtensions))]
    public class EnvironmentExtensionsTests
    {
        [Test]
        public void MapPath_should_combine_web_root_and_relative_path()
        {
            var env = new Mock<IWebHostEnvironment>();
            env.SetupGet(x => x.WebRootPath).Returns("root");

            env.Object.MapPath("files").Should().Be(Path.Combine("root", "files"));
        }
    }
}

