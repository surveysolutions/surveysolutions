using System.Dynamic;
using System.Web.Http.Controllers;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Microsoft.AspNet.SignalR.Hosting;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Owin.Security;
using Moq;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.WebInterview;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.WebInterview;

namespace WB.Tests.Web.TestFactories
{
    public class OtherFactory
    {
        private class ApiControllerCustomization : ICustomization
        {
            public void Customize(IFixture fixture)
            {
                fixture.Inject(new HttpControllerContext());
                fixture.Inject(new HttpRequestContext());
            }
        }

        public Fixture WebApiAutoFixture()
        {
            var autoFixture = new Fixture();
            autoFixture.Customize(new ApiControllerCustomization());
            autoFixture.Customize(new AutoMoqCustomization());
            return autoFixture;
        }

        public WebInterviewHub WebInterviewHub(IStatefulInterview statefulInterview, IQuestionnaireStorage questionnaire, string sectionId = null, IMapper mapper = null)
        {
            var statefulInterviewRepository = SetUp.StatefulInterviewRepository(statefulInterview);
            var questionnaireStorage = questionnaire;
            var webInterviewInterviewEntityFactory = Create.Service.WebInterviewInterviewEntityFactory(autoMapper: mapper);

            var serviceLocator = Mock.Of<IServiceLocator>(sl =>
                sl.GetInstance<IStatefulInterviewRepository>() == statefulInterviewRepository
                && sl.GetInstance<IQuestionnaireStorage>() == questionnaireStorage
                && sl.GetInstance<IWebInterviewInterviewEntityFactory>() == webInterviewInterviewEntityFactory
                && sl.GetInstance<IAuthorizedUser>() == Mock.Of<IAuthorizedUser>());

            var webInterviewHub = new WebInterviewHub();
            webInterviewHub.SetServiceLocator(serviceLocator);

            webInterviewHub.Context = Mock.Of<HubCallerContext>(h =>
                h.QueryString == Mock.Of<INameValueCollection>(p => 
                    p["interviewId"] == statefulInterview.Id.FormatGuid()
                )
            );

            if (!string.IsNullOrEmpty(sectionId))
            {
                dynamic mockCaller = new ExpandoObject();
                mockCaller.sectionId = sectionId;
                var mockClients = new Mock<IHubCallerConnectionContext<dynamic>>();
                mockClients.Setup(m => m.Caller).Returns((ExpandoObject)mockCaller);
                webInterviewHub.Clients = mockClients.Object;
            }

            return webInterviewHub;
        }

        public HqSignInManager HqSignInManager()
        {
            return new HqSignInManager(Abc.Create.Storage.HqUserManager(), Mock.Of<IAuthenticationManager>(),
                Mock.Of<IHashCompatibilityProvider>());
        }
    }
}
