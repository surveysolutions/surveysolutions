﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Implementation.ReadSide.Indexes;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.FunctionalDenormalization;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;

namespace WB.UI.Headquarters.Code
{
    internal class RevalidateInterviewsAdministrationService : IRevalidateInterviewsAdministrationService
    {
        private static bool areInterviewsBeingRevalidatingNow = false;
        private static bool shouldStopInterviewsRevalidating = false;

        private static readonly object RebuildAllViewsLockObject = new object();
        private static string statusMessage;

        private readonly ICommandService commandService;
        private readonly ILogger logger;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviews;
        private readonly IReadSideRepositoryIndexAccessor indexAccessor;
        private readonly IReadSideRepositoryWriter<ViewWithSequence<InterviewData>> interviewsDataWriter;
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewsSummaryWriter;


        static RevalidateInterviewsAdministrationService()
        {
            UpdateStatusMessage("No administration operations were performed so far.");
        }

        public RevalidateInterviewsAdministrationService(
            ILogger logger,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviews,
            ICommandService commandService, IReadSideRepositoryIndexAccessor indexAccessor, 
            IReadSideRepositoryWriter<ViewWithSequence<InterviewData>> interviewsDataWriter, 
            IReadSideRepositoryWriter<InterviewSummary> interviewsSummaryWriter)
        {
            this.logger = logger;
            this.interviews = interviews;
            this.commandService = commandService;
            this.indexAccessor = indexAccessor;
            this.interviewsDataWriter = interviewsDataWriter;
            this.interviewsSummaryWriter = interviewsSummaryWriter;
        }

        public void RevalidateAllInterviewsWithErrorsAsync()
        {
            new Task(this.RevalidateAllInterviewsWithErrors).Start();
        }

        public bool AreInterviewsBeingRevalidatingNow()
        {
            return areInterviewsBeingRevalidatingNow;
        }

        public string GetReadableStatus()
        {
            return string.Format("{1}{0}{0}Are views being rebuilt now: {2}{0}{0}",
                Environment.NewLine,
                statusMessage,
                areInterviewsBeingRevalidatingNow ? "Yes" : "No");
        }

        private void RevalidateAllInterviewsWithErrors()
        {
            if (!areInterviewsBeingRevalidatingNow)
            {
                lock (RebuildAllViewsLockObject)
                {
                    if (!areInterviewsBeingRevalidatingNow)
                    {
                        this.RevalidateAllInterviewsImpl();
                    }
                }
            }
        }

        public void StopInterviewsRevalidating()
        {
            if (!areInterviewsBeingRevalidatingNow)
                return;

            shouldStopInterviewsRevalidating = true;
        }

        private void RevalidateAllInterviewsImpl()
        {
            try
            {
                var bus = NcqrsEnvironment.Get<IEventBus>() as IEventDispatcher;
                if (bus == null)
                {
                    UpdateStatusMessage("Environments setup problems.");
                    return;
                }

                areInterviewsBeingRevalidatingNow = true;

                string indexName = typeof(CompleteInterviewsWithErrorsIndex).Name;

                var items = this.indexAccessor.Query<InterviewSummary>(indexName);

                UpdateStatusMessage("Determining count of interview to be revalidated.");

                ThrowIfShouldStopViewsRebuilding();

                int allInterviewsCount = items.Count();

                int processedInterviewsCount = 0;

                int pageSize = 50;

                int totalPages = (int)Math.Ceiling(allInterviewsCount / (double)pageSize);

                string revalidationgDetails = "<<NO DETAILS>>";

                DateTime revalidationStarted = DateTime.Now;

                try
                {
                    for (int i = 0; i < totalPages; i++)
                    {
                        UpdateStatusMessage("Acquiring portion of interviews to be revalidated. " + GetReadableRevalidationgDetails(revalidationStarted, processedInterviewsCount, allInterviewsCount, 0));

                        var interviewItemIds = items.Skip(i * pageSize).Take(pageSize).ToList();

                        foreach (var interviewItemId in interviewItemIds)
                        {
                            ThrowIfShouldStopViewsRebuilding();

                            UpdateStatusMessage(string.Format("Revalidated interviews {0}. ", processedInterviewsCount + 1) + GetReadableRevalidationgDetails(revalidationStarted, processedInterviewsCount, allInterviewsCount, 0));

                            this.commandService.Execute(new ReevaluateSynchronizedInterview(interviewItemId.InterviewId));

                            processedInterviewsCount++;
                        }
                    }

                    revalidationgDetails = GetReadableRevalidationgDetails(revalidationStarted, processedInterviewsCount, allInterviewsCount, 0);
                }
                finally
                {
                    UpdateStatusMessage("Revalidating all interviews succeeded." + Environment.NewLine + revalidationgDetails);
                }
            }
            catch (Exception exception)
            {
                this.SaveErrorForStatusReport("Unexpected error occurred", exception);
                UpdateStatusMessage(string.Format("Unexpectedly failed. Last status message:{0}{1}", Environment.NewLine, statusMessage));
                throw;
            }
            finally
            {
                areInterviewsBeingRevalidatingNow = false;
            }

        }

        private static string GetReadableRevalidationgDetails(DateTime revalidatingStarted, int processedInterviewsCount, int allInterviewsCount, int failedInterviewsCount)
        {
            TimeSpan republishTimeSpent = DateTime.Now - revalidatingStarted;

            int speedInInterviewsPerMinute = (int)(
                republishTimeSpent.TotalSeconds == 0
                ? 0
                : 60 * processedInterviewsCount / republishTimeSpent.TotalSeconds);

            TimeSpan estimatedTotalRepublishTime = TimeSpan.FromMilliseconds(
                processedInterviewsCount == 0
                ? 0
                : republishTimeSpent.TotalMilliseconds / processedInterviewsCount * allInterviewsCount);

            return string.Format(
                "Processed interview: {1}. Total interview: {2}. Failed interviews: {3}.{0}Time spent republishing: {4}. Speed: {5} interviews per minute. Estimated time: {6}.",
                Environment.NewLine,
                processedInterviewsCount, allInterviewsCount, failedInterviewsCount,
                republishTimeSpent.ToString(@"hh\:mm\:ss"), speedInInterviewsPerMinute, estimatedTotalRepublishTime.ToString(@"hh\:mm\:ss"));
        }


        private static void ThrowIfShouldStopViewsRebuilding()
        {
            if (shouldStopInterviewsRevalidating)
            {
                shouldStopInterviewsRevalidating = false;
                throw new Exception("Interview revalidating stopped by request.");
            }
        }

        private static void UpdateStatusMessage(string newMessage)
        {
            statusMessage = string.Format("{0}: {1}", DateTime.Now, newMessage);
        }

        private void SaveErrorForStatusReport(string message, Exception exception)
        {
            this.logger.Error(message, exception);
        }
    }
}
