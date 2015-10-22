using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.SynchronizationLog;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.Synchronization;
using WB.Core.Synchronization.MetaInfo;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer
{
    [ApiBasicAuth]
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
        private readonly IInterviewInformationFactory interviewerInterviewsFactory;

        public InterviewerInterviewsController(
            IPlainInterviewFileStorage plainInterviewFileStorage,
            IGlobalInfoProvider globalInfoProvider,
            IInterviewInformationFactory interviewerInterviewsFactory,
            IIncomingSyncPackagesQueue incomingSyncPackagesQueue,
            ICommandService commandService,
            IQueryableReadSideRepositoryReader<InterviewSyncPackageMeta> syncPackagesMetaReader,
            IMetaInfoBuilder metaBuilder,
            ISerializer serializer)
        {
            this.plainInterviewFileStorage = plainInterviewFileStorage;
            this.globalInfoProvider = globalInfoProvider;
            this.interviewerInterviewsFactory = interviewerInterviewsFactory;
            this.incomingSyncPackagesQueue = incomingSyncPackagesQueue;
            this.commandService = commandService;
            this.syncPackagesMetaReader = syncPackagesMetaReader;
            this.metaBuilder = metaBuilder;
            this.serializer = serializer;
        }

        [HttpGet]
        [Route("")]
        public List<InterviewApiView> Get()
        {
            return this.interviewerInterviewsFactory.GetInProgressInterviews(this.globalInfoProvider.GetCurrentUser().Id)
                    .Select(interview => new InterviewApiView()
                    {
                        Id = interview.Id,
                        QuestionnaireIdentity = interview.QuestionnaireIdentity,
                        IsRejected = interview.IsRejected
                    }).ToList();
        }

        [HttpGet]
        [Route("packages/{lastPackageId?}")]
        [WriteToSyncLog(SynchronizationLogType.GetInterviewPackages)]
        public InterviewPackagesApiView GetPackages(string lastPackageId = null)
        {
            var interviewPackages = this.GetInterviewPackages(
                userId: this.globalInfoProvider.GetCurrentUser().Id,
                lastSyncedPackageId: lastPackageId);

            var interviewsByPackages = this.interviewerInterviewsFactory.GetInterviewsByIds(
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
                var interviewSynchronizationDto = this.interviewerInterviewsFactory.GetInterview(packageMetaInformation.InterviewId);

                interviewSynchronizationPackage.Content = this.serializer.Serialize(interviewSynchronizationDto);
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

        [HttpPost]
        [Route("package/{id}/logstate")]
        [WriteToSyncLog(SynchronizationLogType.InterviewPackageProcessed)]
        public void LogPackageAsSuccessfullyHandled(string id)
        {
            InterviewSyncPackageMeta syncPackage = syncPackagesMetaReader.GetById(id);
            if (syncPackage != null && syncPackage.ItemType == SyncItemType.Interview)
            {
                commandService.Execute(new MarkInterviewAsReceivedByInterviewer(syncPackage.InterviewId, GlobalInfo.GetCurrentUser().Id));
            }
        }

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
                throw new SyncPackageNotFoundException(string.Format("Sync package with id {0} was not found on server", lastSyncedPackageId));
            }

            long lastSyncedSortIndex = lastSyncedPackage.SortIndex;

            return groupedQuery(userId, lastSyncedSortIndex).ToList();
        }
    }
}