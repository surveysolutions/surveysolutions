using Moq;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.Synchronization.MetaInfo;
using WB.UI.Headquarters.API.DataCollection.Interviewer.v2;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiTests.InterviewerInterviewsControllerTests.v2
{
    internal class InterviewsApiV2ControllerTestsContext
    {
        public static InterviewsApiV2Controller CreateInterviewerInterviewsController(
            IImageFileStorage imageFileStorage = null,
            IAudioFileStorage audioFileStorage = null,
            IAuthorizedUser authorizedUser = null,
            IInterviewInformationFactory interviewsFactory = null,
            IInterviewPackagesService incomingSyncPackagesQueue = null,
            ICommandService commandService = null,
            IMetaInfoBuilder metaBuilder = null,
            IJsonAllTypesSerializer synchronizationSerializer =  null,
            SyncSettings synchronizationsettings = null)
        {
            return new InterviewsApiV2Controller(
                imageFileStorage: imageFileStorage ?? Mock.Of<IImageFileStorage>(),
                audioFileStorage: audioFileStorage ?? Mock.Of<IAudioFileStorage>(),
                authorizedUser: authorizedUser ?? Mock.Of<IAuthorizedUser>(),
                interviewsFactory: interviewsFactory ?? Mock.Of<IInterviewInformationFactory>(),
                packagesService: incomingSyncPackagesQueue ?? Mock.Of<IInterviewPackagesService>(),
                commandService: commandService ?? Mock.Of<ICommandService>(),
                metaBuilder: metaBuilder ?? Mock.Of<IMetaInfoBuilder>(),
                synchronizationSerializer: synchronizationSerializer ?? Mock.Of<IJsonAllTypesSerializer>(),
                eventStore: Mock.Of<IHeadquartersEventStore>());
        }
    }
}
