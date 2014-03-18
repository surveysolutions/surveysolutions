using Machine.Specifications;
using Ncqrs.Commanding.ServiceModel;
using NSubstitute;
using WB.Core.BoundedContexts.Headquarters.ViewFactories;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.Tests.Controllers.SurveysControllerTests
{
    [Subject(typeof(SurveysController))]
    internal class SurveysControllerTestsContext
    {
        protected static SurveysController CreateSurveysController(
            ISurveyViewFactory surveyViewFactory = null, ICommandService commandService = null)
        {
            return new SurveysController(
                surveyViewFactory ?? Substitute.For<ISurveyViewFactory>(),
                commandService ?? Substitute.For<ICommandService>());
        }

        protected static SurveyDetailsView CreateSurveyDetailsView(string surveyId, string surveyTitle)
        {
            return new SurveyDetailsView
            {
                SurveyId = surveyId,
                Name = surveyTitle
            };
        }

        protected static SupervisorModel CreateSupervisorModel(string surveyId, string login = null, string password = null, string confirmPassword = null)
        {
            return new SupervisorModel
            {
                SurveyId = surveyId,
                Login = login,
                Password = password,
                ConfirmPassword = confirmPassword
            };
        }
    }
}