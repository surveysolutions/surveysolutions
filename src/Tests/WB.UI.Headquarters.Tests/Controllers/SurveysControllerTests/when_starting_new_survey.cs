using System.Web.Mvc;
using Machine.Specifications;
using Ncqrs.Commanding.ServiceModel;
using NSubstitute;
using WB.Core.BoundedContexts.Headquarters.Commands.Survey;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Models;
using It = Machine.Specifications.It;

namespace WB.UI.Headquarters.Tests.Controllers.SurveysControllerTests
{
    internal class when_starting_new_survey : SurveysControllerTestsContext
    {
        Establish context = () =>
        {
            model = new NewSurveyModel { Name = "New Survey" };

            commandService = Substitute.For<ICommandService>();

            controller = CreateSurveysController(commandService: commandService);
        };

        Because of = () =>
            result = controller.StartNew(model);

        It should_execute_start_new_survey_command_with_survey_name_specified_in_model = () =>
            commandService.Received(1).Execute(Arg.Is<StartNewSurvey>(command => command.Name == model.Name));

        It should_redirect_user = () =>
            result.ShouldBeOfExactType<RedirectToRouteResult>();

        It should_redirect_user_to_survey_details_page = () =>
            ((RedirectToRouteResult)result).RouteValues["Action"].ShouldEqual("Details");

        private static ActionResult result;
        private static SurveysController controller;
        private static NewSurveyModel model;
        private static ICommandService commandService;
    }
}