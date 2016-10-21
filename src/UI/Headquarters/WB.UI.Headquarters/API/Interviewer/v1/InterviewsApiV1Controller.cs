using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.Synchronization.MetaInfo;
using WB.UI.Headquarters.Code;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v1
{
    [ApiBasicAuth(new[] { UserRoles.Operator })]
    [ProtobufJsonSerializer]
    [Obsolete("Since v. 5.7")]
    public class InterviewsApiV1Controller : InterviewsControllerBase
    {
        public InterviewsApiV1Controller(
            IPlainInterviewFileStorage plainInterviewFileStorage,
            IGlobalInfoProvider globalInfoProvider,
            IInterviewInformationFactory interviewsFactory,
            IInterviewPackagesService incomingSyncPackagesQueue,
            ICommandService commandService,
            IMetaInfoBuilder metaBuilder,
            IJsonAllTypesSerializer synchronizationSerializer) : base(
                plainInterviewFileStorage: plainInterviewFileStorage, 
                globalInfoProvider: globalInfoProvider,
                interviewsFactory: interviewsFactory,
                interviewPackagesService: incomingSyncPackagesQueue,
                commandService: commandService,
                metaBuilder: metaBuilder,
                synchronizationSerializer: synchronizationSerializer)
        {
        }

        [HttpGet]
        public override HttpResponseMessage Get() => base.Get();

        [HttpGet]
        public HttpResponseMessage Details(Guid id)
        {
            var interviewDetails = this.interviewsFactory.GetInProgressInterviewDetails(id);
            if (interviewDetails == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var interviewMetaInfo = this.metaBuilder.GetInterviewMetaInfo(interviewDetails);
            
            var response = this.Request.CreateResponse(new InterviewDetailsApiView
            {
                LastSupervisorOrInterviewerComment = interviewDetails.Comments,
                RejectedDateTime = interviewDetails.RejectDateTime,
                InterviewerAssignedDateTime = interviewDetails.InterviewerAssignedDateTime,
                AnswersOnPrefilledQuestions = interviewMetaInfo.FeaturedQuestionsMeta.Select(ToAnswerOnPrefilledQuestionApiView).ToList(),
                DisabledGroups = interviewDetails.DisabledGroups.Select(this.ToIdentityApiView).ToList(),
                DisabledQuestions = interviewDetails.DisabledQuestions.Select(this.ToIdentityApiView).ToList(),
                InvalidAnsweredQuestions = interviewDetails.InvalidAnsweredQuestions.Select(this.ToIdentityApiView).ToList(),
                ValidAnsweredQuestions = interviewDetails.ValidAnsweredQuestions.Select(this.ToIdentityApiView).ToList(),
                WasCompleted = interviewDetails.WasCompleted,
                RosterGroupInstances = interviewDetails.RosterGroupInstances.Select(this.ToRosterApiView).ToList(),
                Answers = interviewDetails.Answers.Select(this.ToInterviewApiView).ToList(),
                FailedValidationConditions = this.synchronizationSerializer.Serialize(interviewDetails.FailedValidationConditions.ToList())
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
        [WriteToSyncLog(SynchronizationLogType.PostPackage)]
        public void Post(Guid id, [FromBody]string package) => this.interviewPackagesService.StoreOrProcessPackage(item: package);
        [HttpPost]
        public override void PostImage(PostFileRequest request) => base.PostImage(request);

        private InterviewAnswerApiView ToInterviewApiView(AnsweredQuestionSynchronizationDto answer)
        {
            return new InterviewAnswerApiView
            {
                QuestionId = answer.Id,
                QuestionRosterVector = answer.QuestionRosterVector,
                JsonAnswer = this.synchronizationSerializer.Serialize(answer.Answer)
            };
        }

        private static InterviewAnswerOnPrefilledQuestionApiView ToAnswerOnPrefilledQuestionApiView(FeaturedQuestionMeta metaAnswer)
        {
            return new InterviewAnswerOnPrefilledQuestionApiView
            {
                QuestionId = metaAnswer.PublicKey,
                Answer = metaAnswer.Value
            };
        }

        private RosterApiView ToRosterApiView(KeyValuePair<InterviewItemId, RosterSynchronizationDto[]> roster)
        {
            return new RosterApiView
            {
                Identity = new IdentityApiView
                {
                    QuestionId = roster.Key.Id,
                    RosterVector = roster.Key.InterviewItemRosterVector.ToList()
                },
                Instances = roster.Value.Select(this.ToRosterInstanceApiView).ToList()
            };
        }

        private RosterInstanceApiView ToRosterInstanceApiView(RosterSynchronizationDto rosterInstance)
        {
            return new RosterInstanceApiView
            {
                RosterId = rosterInstance.RosterId,
                OuterScopeRosterVector = rosterInstance.OuterScopeRosterVector.ToList(),
                RosterInstanceId = rosterInstance.RosterInstanceId,
                RosterTitle = rosterInstance.RosterTitle,
                SortIndex = rosterInstance.SortIndex
            };
        }


        private IdentityApiView ToIdentityApiView(InterviewItemId identity)
        {
            return new IdentityApiView
            {
                QuestionId = identity.Id,
                RosterVector = identity.InterviewItemRosterVector.ToList()
            };
        }
    }
}