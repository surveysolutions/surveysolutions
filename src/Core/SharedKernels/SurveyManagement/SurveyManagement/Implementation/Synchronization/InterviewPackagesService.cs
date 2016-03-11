using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Events;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views;
using WB.Core.Synchronization;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization
{
    internal class InterviewPackagesService : IInterviewPackagesService
    {
        private readonly IPlainStorageAccessor<InterviewPackage> interviewPackageStorage;
        private readonly IPlainStorageAccessor<BrokenInterviewPackage> brokenInterviewPackageStorage;
        private readonly ILogger logger;
        private readonly ISerializer serializer;
        private readonly IArchiveUtils archiver;
        private readonly ICommandService commandService;
        private readonly SyncSettings syncSettings;

        public InterviewPackagesService(
            IPlainStorageAccessor<InterviewPackage> interviewPackageStorage,
            IPlainStorageAccessor<BrokenInterviewPackage> brokenInterviewPackageStorage,
            ILogger logger, 
            ISerializer serializer,
            IArchiveUtils archiver,
            ICommandService commandService,
            SyncSettings syncSettings)
        {
            this.interviewPackageStorage = interviewPackageStorage;
            this.brokenInterviewPackageStorage = brokenInterviewPackageStorage;
            this.logger = logger;
            this.serializer = serializer;
            this.archiver = archiver;
            this.commandService = commandService;
            this.syncSettings = syncSettings;
        }

        public virtual void Enqueue(Guid interviewId, string packageContent)
        {
                var interviewPackageId = Guid.NewGuid().FormatGuid();
                this.interviewPackageStorage.Store(new InterviewPackage
                {
                    Id = interviewPackageId,
                    InterviewId = interviewId,
                    IncomingDate = DateTime.UtcNow,
                    PackageContent = packageContent
                }, interviewPackageId);
        }

        public virtual int QueueLength => this.interviewPackageStorage.Query(packages => packages.Count());

        public virtual bool HasPackagesByInterviewId(Guid interviewId)
            => this.interviewPackageStorage.Query(packages => packages.Any(package => package.InterviewId == interviewId)) ||
                   this.brokenInterviewPackageStorage.Query(packages => packages.Any(package => package.InterviewId == interviewId));

        private void DeletePackage(string packageId) => this.interviewPackageStorage.Remove(packageId);

        protected void MovePackageToBrokenPackages(string packageId, Exception exception)
        {
            var interviewPackage = this.interviewPackageStorage.GetById(packageId);

            this.brokenInterviewPackageStorage.Store(new BrokenInterviewPackage
            {
                Id = interviewPackage.Id,
                InterviewId = interviewPackage.InterviewId,
                IncomingDate = interviewPackage.IncomingDate,
                ProcessingDate = DateTime.UtcNow,
                PackageContent = interviewPackage.PackageContent,
                ExceptionType = (exception as InterviewException)?.ExceptionType.ToString() ?? "Unexpected",
                ExceptionMessage = exception.Message,
                ExceptionStackTrace = string.Join(Environment.NewLine, exception.UnwrapAllInnerExceptions().Select(ex => $"{ex.Message} {ex.StackTrace}"))
            }, interviewPackage.Id);

            this.interviewPackageStorage.Remove(packageId);
        }

        public virtual IReadOnlyCollection<string> GetTopSyncItemsAsFileNames(int count) => this.interviewPackageStorage.Query(
                packages => packages.OrderByDescending(package => package.IncomingDate).Select(package => package.Id).Take(count).ToList());

        public virtual void ProcessPackage(string packageId)
        {
            try
            {
                var fileContent = this.interviewPackageStorage.GetById(packageId)?.PackageContent;

                var package = this.serializer.Deserialize<SyncItem>(fileContent);

                var interviewMeta = this.serializer.Deserialize<InterviewMetaInfo>(this.archiver.DecompressString(package.MetaInfo));

                var newInterviewEvents = this.serializer
                    .Deserialize<AggregateRootEvent[]>(this.archiver.DecompressString(package.Content))
                    .Select(e => e.Payload)
                    .ToArray();

                this.commandService.Execute(new SynchronizeInterviewEventsCommand(
                    interviewId: interviewMeta.PublicKey,
                    userId: interviewMeta.ResponsibleId,
                    questionnaireId: interviewMeta.TemplateId,
                    questionnaireVersion: interviewMeta.TemplateVersion,
                    interviewStatus: (InterviewStatus)interviewMeta.Status,
                    createdOnClient: interviewMeta.CreatedOnClient ?? false,
                    synchronizedEvents: newInterviewEvents), syncSettings.Origin);

                this.DeletePackage(packageId);
            }
            catch (Exception e)
            {
                this.logger.Error($"Sync package '{packageId}' wasn't parsed. Reason: '{e.Message}'", e);

                this.MovePackageToBrokenPackages(packageId, e);
            }
        }
    }
}