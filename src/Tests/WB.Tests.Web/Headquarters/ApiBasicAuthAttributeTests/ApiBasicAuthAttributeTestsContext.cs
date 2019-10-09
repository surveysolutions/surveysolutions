﻿using System;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.Hosting;
using Main.Core.Entities.SubEntities;
using Microsoft.Owin.Security;
using Moq;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.UI.Headquarters.Code;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiBasicAuthAttributeTests
{
    internal class ApiBasicAuthAttributeTestsContext
    {
        protected static ApiBasicAuthAttribute CreateApiBasicAuthAttribute()
        {
            return new ApiBasicAuthAttribute(UserRoles.Interviewer);
        }

        protected static HttpActionContext CreateActionContext(Func<string, string, bool> isUserValid = null,
            IUserRepository userStore = null)
        {
            var context =
                new HttpActionContext(
                    new HttpControllerContext(new HttpRequestContext(), new HttpRequestMessage(new HttpMethod("POST"), new Uri("http://hq.org/api/sync")),
                        new HttpControllerDescriptor(), Mock.Of<IHttpController>()), new ReflectedHttpActionDescriptor());


            var hqUserManager = Create.Storage.HqUserManager(userStore);
            var auth = new Mock<IAuthenticationManager>();
            var hqSignInManager = new HqSignInManager(hqUserManager, auth.Object, Mock.Of<IHashCompatibilityProvider>());

            var scopeMock = new Mock<IDependencyScope>();
            scopeMock.Setup(x => x.GetService(typeof(HqSignInManager))).Returns(hqSignInManager);
            context.Request.Properties.Add(HttpPropertyKeys.DependencyScope, scopeMock.Object);

            return context;
        }
    }
}
