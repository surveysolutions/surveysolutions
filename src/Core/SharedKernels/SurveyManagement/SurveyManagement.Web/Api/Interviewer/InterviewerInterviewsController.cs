using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.SynchronizationLog;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.Synchronization;
using WB.Core.Synchronization.MetaInfo;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer
{
    [ApiBasicAuth(new[] { UserRoles.Operator })]
    [RoutePrefix("api/interviewer/v1/interviews")]
    [ProtobufJsonSerializer]
    public class InterviewerInterviewsController : ApiController
    {
        private readonly IPlainInterviewFileStorage plainInterviewFileStorage;
        private readonly IIncomingSyncPackagesQueue incomingSyncPackagesQueue;
        private readonly ICommandService commandService;
        private readonly IQueryableReadSideRepositoryReader<InterviewSyncPackageMeta> syncPackagesMetaReader;
        private readonly IMetaInfoBuilder metaBuilder;
        private readonly ISerializer serializer;
        private readonly IGlobalInfoProvider globalInfoProvider;
        private readonly IInterviewInformationFactory interviewsFactory;

        public InterviewerInterviewsController(
            IPlainInterviewFileStorage plainInterviewFileStorage,
            IGlobalInfoProvider globalInfoProvider,
            IInterviewInformationFactory interviewsFactory,
            IIncomingSyncPackagesQueue incomingSyncPackagesQueue,
            ICommandService commandService,
            IQueryableReadSideRepositoryReader<InterviewSyncPackageMeta> syncPackagesMetaReader,
            IMetaInfoBuilder metaBuilder,
            ISerializer serializer)
        {
            this.plainInterviewFileStorage = plainInterviewFileStorage;
            this.globalInfoProvider = globalInfoProvider;
            this.interviewsFactory = interviewsFactory;
            this.incomingSyncPackagesQueue = incomingSyncPackagesQueue;
            this.commandService = commandService;
            this.syncPackagesMetaReader = syncPackagesMetaReader;
            this.metaBuilder = metaBuilder;
            this.serializer = serializer;
        }

        [HttpGet]
        [Route("")]
        [WriteToSyncLog(SynchronizationLogType.GetInterviews)]
        public HttpResponseMessage Get()
        {
            var resultValue = this.interviewsFactory.GetInProgressInterviews(this.globalInfoProvider.GetCurrentUser().Id)
                .Select(interview => new InterviewApiView()
                {
                    Id = interview.Id,
                    QuestionnaireIdentity = interview.QuestionnaireIdentity,
                    IsRejected = interview.IsRejected
                }).ToList();

            var response = this.Request.CreateResponse(resultValue);
            response.Headers.CacheControl = new CacheControlHeaderValue()
            {
                Public = false,
                NoCache = true
            };

            return response;
        }

        [HttpGet]
        [Route("{id:guid}")]
        [WriteToSyncLog(SynchronizationLogType.GetInterview)]
        public HttpResponseMessage Details(Guid id)
        {
            var interviewDetails = this.interviewsFactory.GetInterviewDetails(id);
            if (interviewDetails == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var interviewMetaInfo = this.metaBuilder.GetInterviewMetaInfo(interviewDetails);

            var resultValue = new InterviewDetailsApiView
            {
                LastSupervisorOrInterviewerComment = interviewDetails.Comments,
                RejectedDateTime = interviewDetails.RejectDateTime,
                InterviewerAssignedDateTime = interviewDetails.InterviewerAssignedDateTime,
                AnswersOnPrefilledQuestions =
                    interviewMetaInfo.FeaturedQuestionsMeta.Select(ToAnswerOnPrefilledQuestionApiView).ToList(),
                DisabledGroups = interviewDetails.DisabledGroups.Select(this.ToIdentityApiView).ToList(),
                DisabledQuestions = interviewDetails.DisabledQuestions.Select(this.ToIdentityApiView).ToList(),
                InvalidAnsweredQuestions = interviewDetails.InvalidAnsweredQuestions.Select(this.ToIdentityApiView).ToList(),
                ValidAnsweredQuestions = interviewDetails.ValidAnsweredQuestions.Select(this.ToIdentityApiView).ToList(),
                WasCompleted = interviewDetails.WasCompleted,
                RosterGroupInstances = interviewDetails.RosterGroupInstances.Select(this.ToRosterApiView).ToList(),
                Answers = interviewDetails.Answers.Select(this.ToInterviewApiView).ToList()
            };
            var response = this.Request.CreateResponse(resultValue);
            response.Headers.CacheControl = new CacheControlHeaderValue
            {
                Public = false,
                NoCache = true
            };

            return response;
        }

        [HttpPost]
        [Route("{id:guid}/logstate")]
        [WriteToSyncLog(SynchronizationLogType.InterviewProcessed)]
        public void LogInterviewAsSuccessfullyHandled(Guid id)
        {
            this.commandService.Execute(new MarkInterviewAsReceivedByInterviewer(id, this.globalInfoProvider.GetCurrentUser().Id));
        }

        [HttpPost]
        [Route("{id:guid}")]
        [Route("package/{id:guid}")]
        public void Post(Guid id, [FromBody]string package)
        {
            this.incomingSyncPackagesQueue.Enqueue(interviewId: id, item: package);
        }

        [HttpPost]
        [Route("{id:guid}/image")]
        public void PostImage(PostFileRequest request)
        {
            this.plainInterviewFileStorage.StoreInterviewBinaryData(request.InterviewId, request.FileName,
                Convert.FromBase64String(request.Data));
        }

        private InterviewAnswerApiView ToInterviewApiView(AnsweredQuestionSynchronizationDto answer)
        {
            return new InterviewAnswerApiView
            {
                QuestionId = answer.Id,
                QuestionRosterVector = answer.QuestionRosterVector,
                LastSupervisorOrInterviewerComment = answer.Comments,
                JsonAnswer = this.serializer.Serialize(answer.Answer, TypeSerializationSettings.AllTypes)
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
                Instances = roster.Value.Select(ToRosterInstanceApiView).ToList()
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

        #region Remove it when all clients will be version 5.4.0 and more
        [HttpGet]
        [Route("packages/{lastPackageId?}")]
        [WriteToSyncLog(SynchronizationLogType.GetInterviewPackages)]
        [Obsolete]
        public InterviewPackagesApiView GetPackages(string lastPackageId = null)
        {
            var interviewPackages = this.GetInterviewPackages(
                userId: this.globalInfoProvider.GetCurrentUser().Id,
                lastSyncedPackageId: lastPackageId);

            var interviewsByPackages = this.interviewsFactory.GetInterviewsByIds(
                interviewPackages.Where(package => package.ItemType == SyncItemType.Interview)
                    .Select(package => package.InterviewId).Distinct().ToArray());

            return new InterviewPackagesApiView()
            {
                Packages = interviewPackages,
                Interviews = interviewsByPackages.Select(interview => new InterviewApiView()
                {
                    Id = interview.Id,
                    QuestionnaireIdentity = interview.QuestionnaireIdentity,
                    IsRejected = interview.IsRejected
                }).ToList()
            };
        }

        [HttpGet]
        [Route("package/{id}")]
        [WriteToSyncLog(SynchronizationLogType.GetInterviewPackage)]
        [Obsolete]
        public InterviewSyncPackageDto GetPackage(string id)
        {
            var packageMetaInformation = this.syncPackagesMetaReader.GetById(id);
            if (packageMetaInformation == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var interviewSynchronizationPackage = new InterviewSyncPackageDto
            {
                PackageId = packageMetaInformation.PackageId
            };

            if (packageMetaInformation.ItemType == SyncItemType.Interview)
            {
                var interviewSynchronizationDto = this.interviewsFactory.GetInterviewDetails(packageMetaInformation.InterviewId);

                interviewSynchronizationPackage.Content = this.serializer.Serialize(interviewSynchronizationDto, TypeSerializationSettings.AllTypes);
                interviewSynchronizationPackage.MetaInfo =
                    this.serializer.Serialize(this.metaBuilder.GetInterviewMetaInfo(interviewSynchronizationDto));
            }
            else
            {
                interviewSynchronizationPackage.Content = packageMetaInformation.InterviewId.FormatGuid();
            }

            return interviewSynchronizationPackage;
        }

        [HttpPost]
        [Route("package/{id}/logstate")]
        [WriteToSyncLog(SynchronizationLogType.InterviewPackageProcessed)]
        [Obsolete]
        public void LogPackageAsSuccessfullyHandled(string id)
        {
            InterviewSyncPackageMeta syncPackage = syncPackagesMetaReader.GetById(id);
            if (syncPackage != null && syncPackage.ItemType == SyncItemType.Interview)
            {
                commandService.Execute(new MarkInterviewAsReceivedByInterviewer(syncPackage.InterviewId, GlobalInfo.GetCurrentUser().Id));
            }
        }

        [Obsolete]
        private List<SynchronizationChunkMeta> GetInterviewPackages(Guid userId, string lastSyncedPackageId)
        {
            IList<InterviewSyncPackageMeta> allUpdatesFromLastPackage =
                this.GetUpdateFromLastPackage(userId, lastSyncedPackageId, GetGroupedInterviewSyncPackage, GetLastInterviewSyncPackage);

            return allUpdatesFromLastPackage.Select(x =>
                new SynchronizationChunkMeta(x.PackageId, x.SortIndex, x.UserId, x.ItemType)
                {
                    InterviewId = x.InterviewId
                }).ToList();
        }

        [Obsolete]
        private IQueryable<InterviewSyncPackageMeta> GetLastInterviewSyncPackage(Guid userId)
        {
            List<InterviewSyncPackageMeta> result = this.syncPackagesMetaReader.Query(_ =>
               _.Where(x => x.UserId == userId).ToList()
            );

            return result.AsQueryable();
        }

        [Obsolete]
        private IQueryable<InterviewSyncPackageMeta> GetGroupedInterviewSyncPackage(Guid userId, long? lastSyncedSortIndex)
        {
            var packages = this.syncPackagesMetaReader.Query(_ =>
            {
                var filteredItems = _.Where(x => x.UserId == userId);

                if (lastSyncedSortIndex.HasValue)
                {
                    filteredItems = filteredItems.Where(x => x.SortIndex > lastSyncedSortIndex.Value);
                }

                return filteredItems.OrderBy(x => x.SortIndex).ThenBy(x => x.PackageId).ToList();
            });

            IEnumerable<InterviewSyncPackageMeta> result =
                from p in packages
                group p by p.InterviewId into g
                select g.Last();

            if (lastSyncedSortIndex == null)
            {
                result = result.Where(x => x.ItemType != SyncItemType.DeleteInterview);
            }

            return result.AsQueryable();
        }

        [Obsolete]
        private IList<T> GetUpdateFromLastPackage<T>(Guid userId, string lastSyncedPackageId,
            Func<Guid, long?, IQueryable<T>> groupedQuery, Func<Guid, IQueryable<T>> allQuery)
            where T : InterviewSyncPackageMeta
        {
            if (lastSyncedPackageId == null)
            {
                return groupedQuery(userId, null).ToList();
            }

            var queryable = allQuery(userId).ToList();
            var lastSyncedPackage = queryable
                .FirstOrDefault(x => x.PackageId == lastSyncedPackageId);

            if (lastSyncedPackage == null)
            {
                throw new SyncPackageNotFoundException(string.Format("Sync package with id {0} was not found on server", lastSyncedPackageId));
            }

            long lastSyncedSortIndex = lastSyncedPackage.SortIndex;

            return groupedQuery(userId, lastSyncedSortIndex).ToList();
        }
#endregion
    }
}