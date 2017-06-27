using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.UI.Designer.Controllers;

using NUnit.Framework;

namespace WB.Tests.Unit.Designer.Applications.QuestionnaireControllerTests
{
    internal class when_editing_cascading_options : QuestionnaireControllerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            controller = CreateQuestionnaireController();
            SetControllerContextWithSession(controller, "options", new QuestionnaireController.EditOptionsViewModel());

            stream = GenerateStreamFromString("1\tStreet 1\t2");

            stream.Position = 0;
            postedFile = Mock.Of<HttpPostedFileBase>(pf => pf.InputStream == stream && pf.FileName == "data.csv");
            BecauseOf();
        }

        private void BecauseOf() => view = controller.EditCascadingOptions(postedFile);

        [NUnit.Framework.Test] public void should_return_list_with_1_option () =>
            ((IEnumerable<Option>)view.Model).Count().ShouldEqual(1);

        [NUnit.Framework.Test] public void should_return_first_option_with_value_equals_1 () =>
            ((IEnumerable<Option>)view.Model).First().Value.ShouldEqual("1");

        [NUnit.Framework.Test] public void should_return_first_option_with_title_equals_Street_1 () =>
            ((IEnumerable<Option>)view.Model).First().Title.ShouldEqual("Street 1");

        [NUnit.Framework.Test]
        public void should_return_first_option_with_parent_value_equals_2() =>
            ((IEnumerable<Option>)view.Model).First().ParentValue.ShouldEqual("2");

        [OneTimeTearDown]
        public void cleanup()
        {
            stream.Dispose();
        }

        private static QuestionnaireController controller;
        private static HttpPostedFileBase postedFile;
        private static Stream stream = new MemoryStream();
        private static ViewResult view;
    }
}