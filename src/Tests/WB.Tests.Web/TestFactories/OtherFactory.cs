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
using WB.Enumerator.Native.WebInterview.Pipeline;
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

        public HqSignInManager HqSignInManager()
        {
            return new HqSignInManager(Abc.Create.Storage.HqUserManager(), Mock.Of<IAuthenticationManager>(),
                Mock.Of<IHashCompatibilityProvider>());
        }
    }
}
