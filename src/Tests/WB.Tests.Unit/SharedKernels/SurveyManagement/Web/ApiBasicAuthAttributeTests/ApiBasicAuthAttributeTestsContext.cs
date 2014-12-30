﻿using System;
using System.Net.Http;
using System.Web.Http.Controllers;
using Moq;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiBasicAuthAttributeTests
{
    internal class ApiBasicAuthAttributeTestsContext
    {
        protected static ApiBasicAuthAttribute Create(Func<string, string, bool> isUserValid = null, IViewFactory<UserViewInputModel, UserView> userViewFactory = null)
        {
            Setup.InstanceToMockedServiceLocator(userViewFactory ?? Mock.Of<IViewFactory<UserViewInputModel, UserView>>());

            return new ApiBasicAuthAttribute(isUserValid);
        }

        protected static HttpActionContext CreateActionContext()
        {
            return
                new HttpActionContext(
                    new HttpControllerContext(new HttpRequestContext(), new HttpRequestMessage(new HttpMethod("POST"), new Uri("http://hq.org/api/sync")),
                        new HttpControllerDescriptor(), Mock.Of<IHttpController>()), new ReflectedHttpActionDescriptor());
        }
    }
}