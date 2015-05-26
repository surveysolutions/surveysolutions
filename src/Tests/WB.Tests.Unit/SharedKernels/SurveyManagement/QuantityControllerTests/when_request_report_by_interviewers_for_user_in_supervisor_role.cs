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
using It = Machine.Specifications.It;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuantityControllerTests
{
    internal class when_request_report_by_interviewers_for_user_in_supervisor_role : QuantityControllerTestContext
    {
        Establish context = () =>
        {
            IGlobalInfoProvider globalInfoProvider = Mock.Of<IGlobalInfoProvider>(_ => _.IsSurepvisor == true);
            quantityController = CreateQuantityController(globalInfoProvider: globalInfoProvider);
        };

        Because of = () =>
            result = quantityController.ByInterviewers(null) as ViewResult;

        It should_active_page_be_NumberOfCompletedInterviews = () =>
            ((MenuItem)result.ViewBag.ActivePage).ShouldEqual(MenuItem.NumberOfCompletedInterviews);

        It should_responsible_name_be_not_a_link = () =>
            ((bool)result.ViewBag.CanNavigateToQuantityByTeamMember).ShouldEqual(false);

        It should_go_back_to_supervisor_button_be_invisible = () =>
            ((bool)result.ViewBag.CanNavigateToQuantityBySupervisors).ShouldEqual(false);

        It should_WebApiActionName_be_QuantityByInterviewers = () =>
           ((string)result.ViewBag.WebApiActionName).ShouldEqual("QuantityByInterviewers");
            
        private static QuantityController quantityController;
        private static ViewResult result;
    }
}
