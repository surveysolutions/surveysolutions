using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.Synchronization.MetaInfo;
using WB.Tests.Abc;
using WB.UI.Headquarters.Controllers.Api.DataCollection.Interviewer.v3;

namespace WB.Tests.Web.Headquarters.Controllers.InterviewerInterviewsControllerTests.v3
{
    [TestFixture]
    public class InterviewsApiV3ControllerTests
    {
        [Test]
        public void when_old_interviewer_get_interview_with_short_substitution_events_should_return_update_required()
        {
            var interviewId = Id.g1;
            var userAgent = "org.worldbank.solutions.interviewer/20.04.111 (build 25532) (DEBUG QuestionnaireVersion/29.0.0)";

            var events = new CommittedEvent[]
            {
                Abc.Create.Event.InterviewCreated().ToCommittedEvent(),
                Abc.Create.Event.TextQuestionAnswered().ToCommittedEvent(),
                Abc.Create.Event.SubstitutionTitlesChanged().ToCommittedEvent(),
            };
            var eventStore = Mock.Of<IHeadquartersEventStore>(s => s.Read(interviewId, 0) == events);
            var webHostEnvironment = Mock.Of<IWebHostEnvironment>(h => h.EnvironmentName == Environments.Production);
            var controller = CreateInterviewerInterviewsController(
                eventStore: eventStore,
                webHostEnvironment: webHostEnvironment,
                userAgent: userAgent);

            // act
            var response = controller.Details(interviewId);

            // assert
            Assert.That(response.GetType(), Is.EqualTo(typeof(StatusCodeResult)));
            Assert.That(((StatusCodeResult)response).StatusCode, Is.EqualTo(StatusCodes.Status426UpgradeRequired));
        }

        public static InterviewsApiV3Controller CreateInterviewerInterviewsController(
            IImageFileStorage imageFileStorage = null,
            IAudioFileStorage audioFileStorage = null,
            IAudioAuditFileStorage audioAuditFileStorage = null,
            IAuthorizedUser authorizedUser = null,
            IInterviewInformationFactory interviewsFactory = null,
            IInterviewPackagesService incomingSyncPackagesQueue = null,
            ICommandService commandService = null,
            IMetaInfoBuilder metaBuilder = null,
            IJsonAllTypesSerializer synchronizationSerializer = null,
            IHeadquartersEventStore eventStore = null,
            IUserToDeviceService userToDeviceService = null,
            IWebHostEnvironment webHostEnvironment = null,
            string userAgent = null)
        {
            var httpContext = new DefaultHttpContext(); // or mock a `HttpContext`
            if (userAgent != null)
                httpContext.Request.Headers["User-Agent"] = userAgent;

            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };

            var controller = new InterviewsApiV3Controller(
                imageFileStorage: imageFileStorage ?? Mock.Of<IImageFileStorage>(),
                audioFileStorage: audioFileStorage ?? Mock.Of<IAudioFileStorage>(),
                authorizedUser: authorizedUser ?? Mock.Of<IAuthorizedUser>(),
                interviewsFactory: interviewsFactory ?? Mock.Of<IInterviewInformationFactory>(),
                packagesService: incomingSyncPackagesQueue ?? Mock.Of<IInterviewPackagesService>(),
                commandService: commandService ?? Mock.Of<ICommandService>(),
                metaBuilder: metaBuilder ?? Mock.Of<IMetaInfoBuilder>(),
                synchronizationSerializer: synchronizationSerializer ?? Mock.Of<IJsonAllTypesSerializer>(),
                eventStore: eventStore ?? Mock.Of<IHeadquartersEventStore>(),
                audioAuditFileStorage: audioAuditFileStorage ?? Mock.Of<IAudioAuditFileStorage>(),
                userToDeviceService ?? Mock.Of<IUserToDeviceService>(),
                webHostEnvironment: webHostEnvironment ?? Mock.Of<IWebHostEnvironment>());
            controller.ControllerContext = controllerContext;
            return controller;
        }
    }
}
