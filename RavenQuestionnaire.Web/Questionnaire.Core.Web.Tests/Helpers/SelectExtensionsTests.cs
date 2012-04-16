using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using Questionnaire.Core.Web.Helpers;

namespace Questionnaire.Core.Web.Tests.Helpers
{
    [TestFixture]
    public class SelectExtensionsTests
    {

        public class TestModel
        {
            public TestModel()
            {
                Items = new Dictionary<string, string>();
                Items.Add("one", "one");
            }

            public Dictionary<string, string> Items { get; set; }
        }

        [Test]
        public void CreateHtmlHelper()
        {
            TestModel myTest = new TestModel();
            myTest.Items.Add("first", "first");
            myTest.Items.Add("second", "second");
            myTest.Items.Add("third", "third");
            ViewDataDictionary vd = new ViewDataDictionary(myTest);
            var controllerContext = new ControllerContext(new Mock<HttpContextBase>().Object,
                                                          new RouteData(),
                                                          new Mock<ControllerBase>().Object);
            var viewContext = new ViewContext(controllerContext, new Mock<IView>().Object, vd, new TempDataDictionary(), new Mock<TextWriter>().Object);
            var mockViewDataContainer = new Mock<IViewDataContainer>();
            mockViewDataContainer.Setup(v => v.ViewData).Returns(vd);
            var hh = new HtmlHelper<TestModel>(viewContext, mockViewDataContainer.Object);
            var result = hh.ExtendedDropDownListFor(c=>c.Items).ToHtmlString();
            Assert.AreEqual("<select><option>one</option><option>second</option><option>third</option></select>", result);
        }

        [Test]
        public void CreatesHtmlHelper()
        {
            ViewDataDictionary vd = new ViewDataDictionary(new TestModel());
            var controllerContext = new ControllerContext(new Mock<HttpContextBase>().Object,
                                                          new RouteData(),
                                                          new Mock<ControllerBase>().Object);
            var viewContext = new ViewContext(controllerContext, new Mock<IView>().Object, vd, new TempDataDictionary(), new Mock<TextWriter>().Object);
            var mockViewDataContainer = new Mock<IViewDataContainer>();
            mockViewDataContainer.Setup(v => v.ViewData).Returns(vd);
            var hh = new HtmlHelper<TestModel>(viewContext, mockViewDataContainer.Object).ExtendedDropDownListFor(c=>c.Items).ToHtmlString();
            //var s=hh.ExtendedDropDownListFor(c => c.Items);
            Assert.AreEqual("<select><option>one</option><option>second</option><option>third</option></select>", hh);
        }

    }
}
