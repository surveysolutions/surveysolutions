﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.SynchronizationLog;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.Synchronization.MetaInfo;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v2
{
    [ApiBasicAuth(new[] { UserRoles.Operator })]
    public class InterviewsApiV2Controller : InterviewsControllerBase
    {
        public InterviewsApiV2Controller(
            IPlainInterviewFileStorage plainInterviewFileStorage,
            IGlobalInfoProvider globalInfoProvider,
            IInterviewInformationFactory interviewsFactory,
            IInterviewPackagesService incomingSyncPackagesQueue,
            ICommandService commandService,
            IQueryableReadSideRepositoryReader<InterviewSyncPackageMeta> syncPackagesMetaReader,
            IMetaInfoBuilder metaBuilder,
            IJsonAllTypesSerializer synchronizationSerializer) : base(
                plainInterviewFileStorage: plainInterviewFileStorage,
                globalInfoProvider: globalInfoProvider,
                interviewsFactory: interviewsFactory,
                interviewPackagesService: incomingSyncPackagesQueue,
                commandService: commandService,
                syncPackagesMetaReader: syncPackagesMetaReader,
                metaBuilder: metaBuilder,
                synchronizationSerializer: synchronizationSerializer)
        {
        }

        [HttpGet]
        public override HttpResponseMessage Get() => base.Get();

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetInterview)]
        public HttpResponseMessage Details(Guid id)
        {
            var interviewDetails = this.interviewsFactory.GetInterviewDetails(id);
            if (interviewDetails == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var interviewMetaInfo = this.metaBuilder.GetInterviewMetaInfo(interviewDetails);

            var response = this.Request.CreateResponse(new InterviewerInterviewApiView
            {
                AnswersOnPrefilledQuestions = interviewMetaInfo?.FeaturedQuestionsMeta
                    .Select(prefilledQuestion => new AnsweredQuestionSynchronizationDto(prefilledQuestion.PublicKey, new decimal[0], prefilledQuestion.Value, string.Empty))
                    .ToArray(),
                Details = this.synchronizationSerializer.Serialize(interviewDetails)
            });

            response.Headers.CacheControl = new CacheControlHeaderValue
            {
                Public = false,
                NoCache = true
            };

            return response;
        }
        [HttpPost]
        public override void LogInterviewAsSuccessfullyHandled(Guid id) => base.LogInterviewAsSuccessfullyHandled(id);

        [HttpPost]
        [WriteToSyncLog(SynchronizationLogType.PostInterview)]
        public void Post(InterviewPackageApiView package)
        {
            this.interviewPackagesService.StorePackage(
                interviewId: package.InterviewId,
                questionnaireId: package.MetaInfo.TemplateId,
                questionnaireVersion: package.MetaInfo.TemplateVersion,
                responsibleId: package.MetaInfo.ResponsibleId,
                interviewStatus: (InterviewStatus)package.MetaInfo.Status,
                isCensusInterview: package.MetaInfo.CreatedOnClient ?? false,
                events: package.Events);
        }
        [HttpPost]
        public override void PostImage(PostFileRequest request) => base.PostImage(request);
    }
}