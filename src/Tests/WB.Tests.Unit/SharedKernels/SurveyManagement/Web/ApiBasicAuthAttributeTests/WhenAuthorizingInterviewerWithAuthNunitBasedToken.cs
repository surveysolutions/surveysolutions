using System;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiBasicAuthAttributeTests
{
    public class WhenAuthorizingInterviewerWithAuthNunitBasedToken : ApiBasicAuthNunitBasedSpecification
    {
        protected override void Context()
        {
            this.SetupInterviwer(Create.Entity.HqUser(role: UserRoles.Interviewer, passwordHash: "open sesame"));

            this.HashCompatibilityProvider.Setup(h => h.IsInSha1CompatibilityMode()).Returns(false);
            this.ApiTokenProviderProvider.Setup(p => p.ValidateTokenAsync(Moq.It.IsAny<Guid>(), "open sesame"))
                .Returns(Task.FromResult(true));

            this.attribute = this.CreateApiBasicAuthAttribute();
            this.actionContext = CreateActionContext();
            this.actionContext.Request.Headers.Authorization = new AuthenticationHeaderValue("AuthToken", "QWxhZGRpbjpvcGVuIHNlc2FtZQ==");
        }

        [Test]
        public void Should_Authorize_User() => Thread.CurrentPrincipal.ShouldNotBeNull();

        [Test]
        public void Should_not_return_any_errors() => this.actionContext.Response.ShouldBeNull();
        
        [Test]
        public void Should_Validate_using_token()
            => this.ApiTokenProviderProvider.Verify(p => p.ValidateTokenAsync(Moq.It.IsAny<Guid>(), "open sesame"), Times.Once);
    }
}