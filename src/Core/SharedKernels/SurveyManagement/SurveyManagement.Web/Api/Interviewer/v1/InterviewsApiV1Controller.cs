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
            IIncomingSyncPackagesQueue incomingSyncPackagesQueue,
            ICommandService commandService,
            IQueryableReadSideRepositoryReader<InterviewSyncPackageMeta> syncPackagesMetaReader,
            IMetaInfoBuilder metaBuilder,
            ISerializer serializer) : base(
                plainInterviewFileStorage: plainInterviewFileStorage, 
                globalInfoProvider: globalInfoProvider,
                interviewsFactory: interviewsFactory,
                incomingSyncPackagesQueue: incomingSyncPackagesQueue,
                commandService: commandService,
                syncPackagesMetaReader: syncPackagesMetaReader,
                metaBuilder: metaBuilder,
                serializer: serializer)
        {
        }

        [HttpGet]
        public override HttpResponseMessage Get() => base.Get();

        [HttpGet]
        public HttpResponseMessage Details(Guid id)
        {
            var interviewDetails = this.interviewsFactory.GetInterviewDetails(id);
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
                FailedValidationConditions = this.serializer.Serialize(interviewDetails.FailedValidationConditions.ToList())
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
        public void Post(Guid id, [FromBody]string package) => this.incomingSyncPackagesQueue.Enqueue(interviewId: id, item: package);
        [HttpPost]
        public override void PostImage(PostFileRequest request) => base.PostImage(request);

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetInterviewPackages)]
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
        [WriteToSyncLog(SynchronizationLogType.GetInterviewPackage)]
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
        [WriteToSyncLog(SynchronizationLogType.InterviewPackageProcessed)]
        public void LogPackageAsSuccessfullyHandled(string id)
        {
            InterviewSyncPackageMeta syncPackage = this.syncPackagesMetaReader.GetById(id);
            if (syncPackage != null && syncPackage.ItemType == SyncItemType.Interview)
            {
                this.commandService.Execute(new MarkInterviewAsReceivedByInterviewer(syncPackage.InterviewId, GlobalInfo.GetCurrentUser().Id));
            }
        }

        [HttpPost]
        public void PostPackage(Guid id, [FromBody] string package) => this.Post(id, package);

        private List<SynchronizationChunkMeta> GetInterviewPackages(Guid userId, string lastSyncedPackageId)
        {
            IList<InterviewSyncPackageMeta> allUpdatesFromLastPackage =
                this.GetUpdateFromLastPackage(userId, lastSyncedPackageId, this.GetGroupedInterviewSyncPackage, this.GetLastInterviewSyncPackage);

            return allUpdatesFromLastPackage.Select(x =>
                new SynchronizationChunkMeta(x.PackageId, x.SortIndex, x.UserId, x.ItemType)
                {
                    InterviewId = x.InterviewId
                }).ToList();
        }
        
        private IQueryable<InterviewSyncPackageMeta> GetLastInterviewSyncPackage(Guid userId)
        {
            List<InterviewSyncPackageMeta> result = this.syncPackagesMetaReader.Query(_ =>
               _.Where(x => x.UserId == userId).ToList()
            );

            return result.AsQueryable();
        }
        
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
                throw new SyncPackageNotFoundException($"Sync package with id {lastSyncedPackageId} was not found on server");
            }

            long lastSyncedSortIndex = lastSyncedPackage.SortIndex;

            return groupedQuery(userId, lastSyncedSortIndex).ToList();
        }

        private InterviewAnswerApiView ToInterviewApiView(AnsweredQuestionSynchronizationDto answer)
        {
            return new InterviewAnswerApiView
            {
                QuestionId = answer.Id,
                QuestionRosterVector = answer.QuestionRosterVector,
                LastSupervisorOrInterviewerComment = answer.Comments,
                JsonAnswer = this.serializer.Serialize(answer.Answer, SerializationBinderSettings.NewToOld)
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