using System.Linq;
using System.Web.Mvc;
using Machine.Specifications;
using Ncqrs.Commanding.ServiceModel;
using NSubstitute;
using WB.Core.BoundedContexts.Headquarters.Commands.Survey;
using WB.Core.BoundedContexts.Headquarters.Exceptions;
using WB.Core.BoundedContexts.Headquarters.ViewFactories;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.Tests.Controllers.SurveysControllerTests
{
    internal class when_posting_register_supervisor_page_and_AR_throws_domain_exception : SurveysControllerTestsContext
    {
        Establish context = () =>
        {
            var surveyDetails = CreateSurveyDetailsView(surveyId, title);

            var surveyViewFactory = Substitute.For<ISurveyViewFactory>();
            surveyViewFactory.GetDetailsView(surveyId).Returns(surveyDetails);

            model = CreateSupervisorModel(surveyId);

            var commandService = Substitute.For<ICommandService>();

            commandService
                .When(x => x.Execute(Arg.Any<RegisterSupervisor>()))
                .Do(x => { throw new SurveyException(exceptionMessage); });

            controller = CreateSurveysController(surveyViewFactory: surveyViewFactory, commandService: commandService);

            controller.ModelState.Clear();
        };

        Because of = () =>
            result = controller.RegisterSupervisor(surveyId, model);

        It should_set_model_state_IsValid_to_false = () =>
            controller.ModelState.IsValid.ShouldBeFalse();

        It should_set_one_error_message_in_model_state = () =>
            controller.ModelState.Count.ShouldEqual(1);

        It should_set_exceptionMessage_in_model_state = () =>
           controller.ModelState.Values.First().Errors.First().ErrorMessage.ShouldEqual(exceptionMessage);

        It should_return_view_with_model_of_type_SupervisorModel = () =>
            ((ViewResult)result).Model.ShouldBeOfExactType(typeof(SupervisorModel));

        It should_return_view_with_model_with_SurveyId_set_in_surveyId = () =>
            ((SupervisorModel)((ViewResult)result).Model).SurveyId.ShouldEqual(surveyId);

        It should_return_view_with_model_with_SurveyTitle_set_in_surveyId = () =>
            ((SupervisorModel)((ViewResult)result).Model).SurveyTitle.ShouldEqual(title);

        private static ActionResult result;
        private static SurveysController controller;
        private static SupervisorModel model;
        private static string surveyId = "11111111111111111111111111111111";
        private static string title = "some title";
        private static string exceptionMessage = "some message";
    }
}