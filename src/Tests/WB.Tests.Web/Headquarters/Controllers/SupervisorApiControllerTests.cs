using System;
using System.Net;
using System.Web.Http.Dependencies;
using AutoFixture;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.DataCollection.Supervisor.v1;

namespace WB.Tests.Unit.Applications.Headquarters.Api.DataCollection.Supervisor
{
    [TestOf(typeof(SupervisorApiController))]
    internal class SupervisorApiController_CheckCompatibility_Tests
    {
        [Test]
        public void should_force_update_when_resolved_comments_exist()
        {
            Guid authorizedUserId = Id.gA;

            var fixture = Web.Create.Other.WebApiAutoFixture();

            var interviewInformationFactory = Mock.Of<IInterviewInformationFactory>(x =>
                x.HasAnyInterviewsInProgressWithResolvedCommentsForSupervisor(authorizedUserId) == true);
            var authorizedUser = Mock.Of<IAuthorizedUser>(x => x.Id == authorizedUserId);

            fixture.Inject(interviewInformationFactory);
            fixture.Inject(authorizedUser);
            fixture.Freeze<HqSignInManager>(cst => cst.FromFactory(() => Web.Create.Other.HqSignInManager()));

            SupervisorApiController controller = fixture.Create<SupervisorApiController>();

            // Act
            var response = controller.CheckCompatibility("deviceId",
                SupervisorSyncProtocolVersionProvider.V1_BeforeResolvedCommentsIntroduced);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UpgradeRequired));
        }
    }
}
