﻿using Moq;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v2;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.Synchronization;
using WB.Core.Synchronization.MetaInfo;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiTests.InterviewerInterviewsControllerTests.v2
{
    internal class InterviewsApiV2ControllerTestsContext
    {
        public static InterviewsApiV2Controller CreateInterviewerInterviewsController(
            IPlainInterviewFileStorage plainInterviewFileStorage = null,
            IGlobalInfoProvider globalInfoProvider = null,
            IInterviewInformationFactory interviewsFactory = null,
            IIncomingSyncPackagesQueue incomingSyncPackagesQueue = null,
            ICommandService commandService = null,
            IQueryableReadSideRepositoryReader<InterviewSyncPackageMeta> syncPackagesMetaReader = null,
            IMetaInfoBuilder metaBuilder = null,
            ISynchronizationSerializer synchronizationSerializer =  null,
            IStringCompressor compressor = null)
        {
            return new InterviewsApiV2Controller(
                plainInterviewFileStorage: plainInterviewFileStorage ?? Mock.Of<IPlainInterviewFileStorage>(),
                globalInfoProvider: globalInfoProvider ?? Mock.Of<IGlobalInfoProvider>(),
                interviewsFactory: interviewsFactory ?? Mock.Of<IInterviewInformationFactory>(),
                incomingSyncPackagesQueue: incomingSyncPackagesQueue ?? Mock.Of<IIncomingSyncPackagesQueue>(),
                commandService: commandService ?? Mock.Of<ICommandService>(),
                syncPackagesMetaReader: syncPackagesMetaReader ?? Mock.Of<IQueryableReadSideRepositoryReader<InterviewSyncPackageMeta>>(),
                metaBuilder: metaBuilder ?? Mock.Of<IMetaInfoBuilder>(),
                synchronizationSerializer: synchronizationSerializer ?? Mock.Of<ISynchronizationSerializer>(),
                compressor: compressor ?? Mock.Of<IStringCompressor>());
        }
    }
}
