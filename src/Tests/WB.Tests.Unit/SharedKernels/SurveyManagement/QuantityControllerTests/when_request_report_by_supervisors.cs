using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuantityControllerTests
{
    internal class when_request_report_by_supervisors : QuantityControllerTestContext
    {
        Establish context = () =>
        {
            quantityController = CreateQuantityController();
        };

        Because of = () =>
            result = quantityController.BySupervisors() as ViewResult;

        It should_active_page_be_NumberOfCompletedInterviews = () =>
            ((MenuItem)result.ViewBag.ActivePage).ShouldEqual(MenuItem.NumberOfCompletedInterviews);

        It should_responsible_name_be_a_link = () =>
            ((bool)result.ViewBag.IsResponsibleNameLink).ShouldEqual(true);

        It should_go_back_to_supervisor_button_be_invisible = () =>
            ((bool)result.ViewBag.IsGoBackSupervisorsVisible).ShouldEqual(false);

        It should_DataAction_be_QuantityBySupervisors = () =>
           ((string)result.ViewBag.DataAction).ShouldEqual("QuantityBySupervisors");

        private static QuantityController quantityController;
        private static ViewResult result;
    }
}
