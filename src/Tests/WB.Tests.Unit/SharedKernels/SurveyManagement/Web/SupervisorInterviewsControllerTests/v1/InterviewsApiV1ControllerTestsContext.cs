using Moq;
using Ncqrs.Eventing.Storage;
using NUnit.Framework;
using NUnit.Framework.Internal;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.Synchronization.MetaInfo;
using WB.UI.Headquarters.API.DataCollection.Supervisor.v1;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiTests.SupervisorInterviewsControllerTests.v1
{
    [TestFixture(typeof(InterviewsApiV1Controller))]
    internal class InterviewsApiV1ControllerTestsContext
    {
        public static InterviewsApiV1Controller CreateSupervisorInterviewsController(
            IImageFileStorage imageFileStorage = null,
            IAudioFileStorage audioFileStorage = null,
            IAudioAuditFileStorage audioAuditFileStorage = null,
            IAuthorizedUser authorizedUser = null,
            IInterviewInformationFactory interviewsFactory = null,
            IInterviewPackagesService incomingSyncPackagesQueue = null,
            ICommandService commandService = null,
            IMetaInfoBuilder metaBuilder = null,
            IJsonAllTypesSerializer synchronizationSerializer =  null,
            SyncSettings synchronizationsettings = null)
        {
            return new InterviewsApiV1Controller(
                imageFileStorage: imageFileStorage ?? Mock.Of<IImageFileStorage>(),
                audioFileStorage: audioFileStorage ?? Mock.Of<IAudioFileStorage>(),
                authorizedUser: authorizedUser ?? Mock.Of<IAuthorizedUser>(),
                interviewsFactory: interviewsFactory ?? Mock.Of<IInterviewInformationFactory>(),
                packagesService: incomingSyncPackagesQueue ?? Mock.Of<IInterviewPackagesService>(),
                commandService: commandService ?? Mock.Of<ICommandService>(),
                metaBuilder: metaBuilder ?? Mock.Of<IMetaInfoBuilder>(),
                synchronizationSerializer: synchronizationSerializer ?? Mock.Of<IJsonAllTypesSerializer>(),
                eventStore: Mock.Of<IHeadquartersEventStore>(),
                audioAuditFileStorage: audioAuditFileStorage ?? Mock.Of<IAudioAuditFileStorage>());
        }
    }
}
