using Moq;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer;
using WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v1;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.Synchronization;
using WB.Core.Synchronization.MetaInfo;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiTests.InterviewerInterviewsControllerTests
{
    internal class InterviewerInterviewsControllerTestsContext
    {
        public static InterviewsApiV1Controller CreateInterviewerInterviewsController(
            IPlainInterviewFileStorage plainInterviewFileStorage = null,
            IGlobalInfoProvider globalInfoProvider = null,
            IInterviewInformationFactory interviewsFactory = null,
            IIncomingSyncPackagesQueue incomingSyncPackagesQueue = null,
            ICommandService commandService = null,
            IQueryableReadSideRepositoryReader<InterviewSyncPackageMeta> syncPackagesMetaReader = null,
            IMetaInfoBuilder metaBuilder = null,
            ISerializer serializer =  null)
        {
            return new InterviewsApiV1Controller(
                plainInterviewFileStorage: plainInterviewFileStorage ?? Mock.Of<IPlainInterviewFileStorage>(),
                globalInfoProvider: globalInfoProvider ?? Mock.Of<IGlobalInfoProvider>(),
                interviewsFactory: interviewsFactory ?? Mock.Of<IInterviewInformationFactory>(),
                incomingSyncPackagesQueue: incomingSyncPackagesQueue ?? Mock.Of<IIncomingSyncPackagesQueue>(),
                commandService: commandService ?? Mock.Of<ICommandService>(),
                syncPackagesMetaReader: syncPackagesMetaReader ?? Mock.Of<IQueryableReadSideRepositoryReader<InterviewSyncPackageMeta>>(),
                metaBuilder: metaBuilder ?? Mock.Of<IMetaInfoBuilder>(),
                serializer: serializer ?? Mock.Of<ISerializer>());
        }
    }
}
