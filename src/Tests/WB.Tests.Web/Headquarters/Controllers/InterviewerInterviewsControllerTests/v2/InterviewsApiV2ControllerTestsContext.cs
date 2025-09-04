using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.Synchronization.MetaInfo;
using WB.UI.Headquarters.Controllers.Api.DataCollection.Interviewer.v2;
using WB.UI.Shared.Web.Services;

namespace WB.Tests.Web.Headquarters.Controllers.InterviewerInterviewsControllerTests.v2
{
    internal class InterviewsApiV2ControllerTestsContext
    {
        public static InterviewsApiV2Controller CreateInterviewerInterviewsController(
            IImageFileStorage imageFileStorage = null,
            IAudioFileStorage audioFileStorage = null,
            IAudioAuditFileStorage audioAuditFileStorage = null,
            IAuthorizedUser authorizedUser = null,
            IInterviewInformationFactory interviewsFactory = null,
            IInterviewPackagesService incomingSyncPackagesQueue = null,
            ICommandService commandService = null,
            IMetaInfoBuilder metaBuilder = null,
            IJsonAllTypesSerializer synchronizationSerializer =  null)
        {
            var interviewsApiV2Controller = new InterviewsApiV2Controller(
                imageFileStorage: imageFileStorage ?? Mock.Of<IImageFileStorage>(),
                audioFileStorage: audioFileStorage ?? Mock.Of<IAudioFileStorage>(),
                authorizedUser: authorizedUser ?? Mock.Of<IAuthorizedUser>(),
                interviewsFactory: interviewsFactory ?? Mock.Of<IInterviewInformationFactory>(),
                packagesService: incomingSyncPackagesQueue ?? Mock.Of<IInterviewPackagesService>(),
                commandService: commandService ?? Mock.Of<ICommandService>(),
                metaBuilder: metaBuilder ?? Mock.Of<IMetaInfoBuilder>(),
                synchronizationSerializer: synchronizationSerializer ?? Mock.Of<IJsonAllTypesSerializer>(),
                eventStore: Mock.Of<IHeadquartersEventStore>(),
                audioAuditFileStorage: audioAuditFileStorage ?? Mock.Of<IAudioAuditFileStorage>(),
                userToDeviceService: Mock.Of<IUserToDeviceService>(),
                webHostEnvironment: Mock.Of<IWebHostEnvironment>(),
                imageProcessingService: Mock.Of<IImageProcessingService>());

            var httpContext = new DefaultHttpContext(); // or mock a `HttpContext`

            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };

            interviewsApiV2Controller.ControllerContext = controllerContext;

            return interviewsApiV2Controller;
        }
    }
}
