using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Main.Core.Events;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
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
        private readonly IJsonAllTypesSerializer serializer;
        private readonly ICommandService commandService;
        private readonly SyncSettings syncSettings;

        public InterviewPackagesService(
            IPlainStorageAccessor<InterviewPackage> interviewPackageStorage,
            IPlainStorageAccessor<BrokenInterviewPackage> brokenInterviewPackageStorage,
            ILogger logger, 
            IJsonAllTypesSerializer serializer,
            ICommandService commandService,
            SyncSettings syncSettings)
        {
            this.interviewPackageStorage = interviewPackageStorage;
            this.brokenInterviewPackageStorage = brokenInterviewPackageStorage;
            this.logger = logger;
            this.serializer = serializer;
            this.commandService = commandService;
            this.syncSettings = syncSettings;
        }

        [Obsolete("Since v 5.8")]
        public virtual void StorePackage(string item) { }

        public void StorePackage(Guid interviewId, Guid questionnaireId, long questionnaireVersion, Guid responsibleId,
            InterviewStatus interviewStatus, bool isCensusInterview, string events)
        {
                this.interviewPackageStorage.Store(new InterviewPackage
                {
                    InterviewId = interviewId,
                    QuestionnaireId = questionnaireId,
                    QuestionnaireVersion = questionnaireVersion,
                    InterviewStatus = interviewStatus,
                    ResponsibleId = responsibleId,
                    IsCensusInterview = isCensusInterview,
                    IncomingDate = DateTime.UtcNow,
                    Events = events
                }, null);
        }

        public virtual int QueueLength
            => this.interviewPackageStorage.Query(packages => packages.Count());

        public int InvalidPackagesCount
            => this.brokenInterviewPackageStorage.Query(packages => packages.Count());

        public virtual bool HasPendingPackageByInterview(Guid interviewId)
            => this.interviewPackageStorage.Query(packages => packages.Any(package => package.InterviewId == interviewId));

        public void ReprocessAllBrokenPackages()
        {
            List<BrokenInterviewPackage> chunkOfBrokenInterviewPackages;
            do
            {
                chunkOfBrokenInterviewPackages = this.brokenInterviewPackageStorage.Query(brokenPackages => brokenPackages.Take(10).ToList());
                foreach (var brokenInterviewPackage in chunkOfBrokenInterviewPackages)
                {
                    this.interviewPackageStorage.Store(new InterviewPackage
                    {
                        ResponsibleId = brokenInterviewPackage.ResponsibleId,
                        InterviewId = brokenInterviewPackage.InterviewId,
                        IncomingDate = brokenInterviewPackage.IncomingDate,
                        InterviewStatus = brokenInterviewPackage.InterviewStatus,
                        IsCensusInterview = brokenInterviewPackage.IsCensusInterview,
                        QuestionnaireId = brokenInterviewPackage.QuestionnaireId,
                        QuestionnaireVersion = brokenInterviewPackage.QuestionnaireVersion,
                        Events = brokenInterviewPackage.Events
                    }, null);
                    this.brokenInterviewPackageStorage.Remove(brokenInterviewPackage.Id);
                }
            } while (chunkOfBrokenInterviewPackages.Any());
        }

        public virtual IReadOnlyCollection<string> GetAllPackagesInterviewIds()
        {
            var count = int.MaxValue;
            return this.interviewPackageStorage.Query(packages => packages.Select(package => package.InterviewId).Take(count).ToList())
                    .Select(id => id.FormatGuid())
                    .ToReadOnlyCollection();
        }

        public virtual IReadOnlyCollection<string> GetTopPackageIds(int count)
            => this.interviewPackageStorage.Query(packages => packages.Select(package => package.Id).Take(count).ToList())
                    .Select(packageId => packageId.ToString())
                    .ToReadOnlyCollection();

        public virtual void ProcessPackage(string sPackageId)
        {
            int packageId;
            if(int.TryParse(sPackageId, out packageId))
                this.ProcessPackage(packageId);
            else
                this.logger.Warn($"Package {sPackageId}. Unknown package id");
        }

        private void ProcessPackage(int packageId)
        {
            Stopwatch innerwatch = Stopwatch.StartNew();
            InterviewPackage package = null;
            try
            {
                package = this.interviewPackageStorage.GetById(packageId);

                this.logger.Debug($"Package {packageId}. Read content from db. Took {innerwatch.Elapsed:g}.");
                innerwatch.Restart();
                
                var events = this.serializer
                    .Deserialize<AggregateRootEvent[]>(package.Events)
                    .Select(e => e.Payload)
                    .ToArray();

                this.logger.Debug($"Package {packageId}. Decompressed and deserialized. Took {innerwatch.Elapsed:g}.");
                innerwatch.Restart();

                this.commandService.Execute(new SynchronizeInterviewEventsCommand(
                    interviewId: package.InterviewId,
                    userId: package.ResponsibleId,
                    questionnaireId: package.QuestionnaireId,
                    questionnaireVersion: package.QuestionnaireVersion,
                    interviewStatus: package.InterviewStatus,
                    createdOnClient: package.IsCensusInterview,
                    synchronizedEvents: events), syncSettings.Origin);
            }
            catch (Exception exception)
            {
                this.logger.Error($"Package {packageId}. FAILED. Reason: '{exception.Message}'", exception);

                if (package != null)
                {
                    this.brokenInterviewPackageStorage.Store(new BrokenInterviewPackage
                    {
                        InterviewId = package.InterviewId,
                        QuestionnaireId = package.QuestionnaireId,
                        QuestionnaireVersion = package.QuestionnaireVersion,
                        InterviewStatus = package.InterviewStatus,
                        ResponsibleId = package.ResponsibleId,
                        IsCensusInterview = package.IsCensusInterview,
                        IncomingDate = package.IncomingDate,
                        Events = package.Events,
                        PackageSize = package.Events?.Length ?? 0,
                        ProcessingDate = DateTime.UtcNow,
                        ExceptionType = (exception as InterviewException)?.ExceptionType.ToString() ?? "Unexpected",
                        ExceptionMessage = exception.Message,
                        ExceptionStackTrace = string.Join(Environment.NewLine, exception.UnwrapAllInnerExceptions().Select(ex => $"{ex.Message} {ex.StackTrace}"))
                    }, null);

                    this.logger.Debug($"Package {packageId}. Moved to broken packages. Took {innerwatch.Elapsed:g}.");
                    innerwatch.Restart();
                }
            }
            finally
            {
                this.interviewPackageStorage.Remove(packageId);

                this.logger.Debug($"Package {packageId}. Removed. Took {innerwatch.Elapsed:g}.");
                innerwatch.Stop();
            }
        }
    }
}