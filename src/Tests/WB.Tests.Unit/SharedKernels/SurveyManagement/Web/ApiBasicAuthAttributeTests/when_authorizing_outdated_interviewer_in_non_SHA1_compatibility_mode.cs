using System.Net;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiBasicAuthAttributeTests
{
    public class when_authorizing_outdated_interviewer_in_non_SHA1_compatibility_mode : ApiBasicAuthNunitBasedSpecification
    {
        protected override void Context()
        {
            this.SetupInterviwer(Create.Entity.HqUser(role: UserRoles.Interviewer, passwordHashSha1: "open sesame"));
            this.HashCompatibilityProvider.Setup(h => h.IsInSha1CompatibilityMode()).Returns(false);

            this.attribute = this.CreateApiBasicAuthAttribute();
            this.actionContext = CreateActionContext();
            this.actionContext.Request.Headers.Authorization = new AuthenticationHeaderValue("Basic", "QWxhZGRpbjpvcGVuIHNlc2FtZQ==");
        }

        protected override Task BecauseAsync() => this.attribute.OnAuthorizationAsync(this.actionContext, CancellationToken.None);
        
        [Test]
        public void Should_return_upgrade_error() => this.actionContext.Response.StatusCode.Should().Be(HttpStatusCode.UpgradeRequired);
    }
}
