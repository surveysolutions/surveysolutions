﻿using Moq;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v2;
using WB.Core.Synchronization.MetaInfo;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiTests.InterviewerInterviewsControllerTests.v2
{
    internal class InterviewsApiV2ControllerTestsContext
    {
        public static InterviewsApiV2Controller CreateInterviewerInterviewsController(
            IImageQuestionFileStorage imageQuestionFileStorage = null,
            IPlainInterviewFileStorage plainInterviewFileStorage = null,
            IAuthorizedUser authorizedUser = null,
            IInterviewInformationFactory interviewsFactory = null,
            IInterviewPackagesService incomingSyncPackagesQueue = null,
            ICommandService commandService = null,
            IMetaInfoBuilder metaBuilder = null,
            IJsonAllTypesSerializer synchronizationSerializer =  null,
            SyncSettings synchronizationsettings = null)
        {
            return new InterviewsApiV2Controller(
                imageQuestionFileStorage: imageQuestionFileStorage ?? Mock.Of<IImageQuestionFileStorage>(),
                plainInterviewFileStorage: plainInterviewFileStorage ?? Mock.Of<IPlainInterviewFileStorage>(),
                authorizedUser: authorizedUser ?? Mock.Of<IAuthorizedUser>(),
                interviewsFactory: interviewsFactory ?? Mock.Of<IInterviewInformationFactory>(),
                incomingSyncPackagesQueue: incomingSyncPackagesQueue ?? Mock.Of<IInterviewPackagesService>(),
                commandService: commandService ?? Mock.Of<ICommandService>(),
                metaBuilder: metaBuilder ?? Mock.Of<IMetaInfoBuilder>(),
                synchronizationSerializer: synchronizationSerializer ?? Mock.Of<IJsonAllTypesSerializer>());
        }
    }
}
