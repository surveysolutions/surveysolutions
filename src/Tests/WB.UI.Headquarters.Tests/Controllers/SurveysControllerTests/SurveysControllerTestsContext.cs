using Machine.Specifications;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Headquarters.ViewFactories;
using WB.UI.Headquarters.Controllers;

namespace WB.UI.Headquarters.Tests.Controllers.SurveysControllerTests
{
    [Subject(typeof(SurveysController))]
    internal class SurveysControllerTestsContext
    {
        protected static SurveysController CreateSurveysController(
            ISurveyViewFactory surveyViewFactory = null, ICommandService commandService = null)
        {
            return new SurveysController(
                surveyViewFactory ?? Mock.Of<ISurveyViewFactory>(),
                commandService ?? Mock.Of<ICommandService>());
        }
    }
}