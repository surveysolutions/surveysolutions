using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.UI.Designer.Controllers;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Designer.QuestionnaireControllerTests
{
    internal class when_editing_cascading_options : QuestionnaireControllerTestContext
    {
        Establish context = () =>
        {
            controller = CreateQuestionnaireController();
            SetControllerContextWithSession(controller, "options", new QuestionnaireController.EditOptionsViewModel());

            stream = GenerateStreamFromString(@"1,Street 1,2");

            stream.Position = 0;
            postedFile = Mock.Of<HttpPostedFileBase>(pf => pf.InputStream == stream && pf.FileName == "data.csv");
        };

        Because of = () => view = controller.EditCascadingOptions(postedFile);

        It should_return_list_with_1_option = () =>
            ((IEnumerable<Option>)view.Model).Count().ShouldEqual(1);

        It should_return_first_option_with_value_equals_1 = () =>
            ((IEnumerable<Option>)view.Model).First().Value.ShouldEqual("1");

        It should_return_first_option_with_title_equals_Street_1 = () =>
            ((IEnumerable<Option>)view.Model).First().Title.ShouldEqual("Street 1");

        It should_return_first_option_with_parent_value_equals_2= () =>
            ((IEnumerable<Option>)view.Model).First().ParentValue.ShouldEqual("2");

        Cleanup cleanup = () =>
        {
            stream.Dispose();
        };

        private static QuestionnaireController controller;
        private static HttpPostedFileBase postedFile;
        private static Stream stream = new MemoryStream();
        private static ViewResult view;
    }
}