﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Code
{
    internal class RevalidateInterviewsAdministrationService : IRevalidateInterviewsAdministrationService
    {
        private static bool areInterviewsBeingRevalidatingNow = false;
        private static bool shouldStopInterviewsRevalidating = false;

        private static readonly object RebuildAllViewsLockObject = new object();
        private static string statusMessage;
        private readonly ITransactionManagerProvider transactionManagerProvider;
        private readonly IPlainTransactionManagerProvider plainTransactionManagerProvider;

        private readonly ICommandService commandService;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewsReader;
        private readonly ILogger logger;


        static RevalidateInterviewsAdministrationService()
        {
            UpdateStatusMessage("No administration operations were performed so far.");
        }

        public RevalidateInterviewsAdministrationService(
            ILogger logger,
            ICommandService commandService, 
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewsReader, 
            ITransactionManagerProvider transactionManagerProvider,
            IPlainTransactionManagerProvider plainTransactionManagerProvider)
        {
            this.logger = logger;
            this.commandService = commandService;
            this.interviewsReader = interviewsReader;
            this.transactionManagerProvider = transactionManagerProvider;
            this.plainTransactionManagerProvider = plainTransactionManagerProvider;
        }

        public void RevalidateAllInterviewsWithErrorsAsync(Guid userId, DateTime? startDate, DateTime? endDate)
        {
            new Task(() => this.RevalidateAllInterviewsWithErrors(userId, startDate, endDate)).Start();
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

        private void RevalidateAllInterviewsWithErrors(Guid userId, DateTime? startDate, DateTime? endDate)
        {
            if (!areInterviewsBeingRevalidatingNow)
            {
                lock (RebuildAllViewsLockObject)
                {
                    if (!areInterviewsBeingRevalidatingNow)
                    {
                        this.RevalidateAllInterviewsImpl(userId, startDate, endDate);
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

        private void RevalidateAllInterviewsImpl(Guid userId, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var bus = ServiceLocator.Current.GetInstance<ILiteEventBus>();
                if (bus == null)
                {
                    UpdateStatusMessage("Environments setup problems.");
                    return;
                }

                areInterviewsBeingRevalidatingNow = true;

                List<InterviewSummary> interviews = new List<InterviewSummary>();
                transactionManagerProvider.GetTransactionManager().ExecuteInQueryTransaction(() =>
                {
                    interviews = this.interviewsReader.Query(_ =>
                    {
                        if (startDate.HasValue)
                            _ = _.Where(i => i.UpdateDate >= startDate.Value);
                        if (endDate.HasValue)
                            _ = _.Where(i => i.UpdateDate <= endDate.Value);

                        return _.Where(interview =>
                            interview.Status == InterviewStatus.Completed
                            || interview.Status == InterviewStatus.RejectedBySupervisor
                            || interview.Status == InterviewStatus.ApprovedBySupervisor
                            || interview.Status == InterviewStatus.ApprovedByHeadquarters
                            || interview.Status == InterviewStatus.RejectedByHeadquarters).ToList();
                    });
                });
                

                UpdateStatusMessage("Determining count of interview to be revalidated.");

                ThrowIfShouldStopViewsRebuilding();

                int allInterviewsCount = interviews.Count;

                int processedInterviewsCount = 0;

                int pageSize = 50;

                int totalPages = (int)Math.Ceiling(allInterviewsCount / (double)pageSize);

                string revalidationgDetails = "<<NO DETAILS>>";
                int failedInterviewsCount = 0;
                DateTime revalidationStarted = DateTime.Now;

                try
                {
                    for (int i = 0; i < totalPages; i++)
                    {
                        UpdateStatusMessage("Acquiring portion of interviews to be revalidated. " + GetReadableRevalidationgDetails(revalidationStarted, processedInterviewsCount, allInterviewsCount, failedInterviewsCount));

                        var interviewItemIds = interviews.Skip(i * pageSize).Take(pageSize).ToList();

                        foreach (var interviewItemId in interviewItemIds)
                        {
                            ThrowIfShouldStopViewsRebuilding();

                            UpdateStatusMessage(string.Format("Revalidated interviews {0}. ", processedInterviewsCount + 1) + GetReadableRevalidationgDetails(revalidationStarted, processedInterviewsCount, allInterviewsCount, failedInterviewsCount));

                            try
                            {
                                this.transactionManagerProvider.GetTransactionManager().BeginCommandTransaction();
                                this.plainTransactionManagerProvider.GetPlainTransactionManager().BeginTransaction();

                                this.commandService.Execute(new ReevaluateSynchronizedInterview(interviewItemId.InterviewId, userId));

                                this.transactionManagerProvider.GetTransactionManager().CommitCommandTransaction();
                                this.plainTransactionManagerProvider.GetPlainTransactionManager().CommitTransaction();
                            }
                            catch (Exception e)
                            {
                                this.logger.Error($"Failed to revalidate interview {interviewItemId}", e);
                                this.transactionManagerProvider.GetTransactionManager().RollbackCommandTransaction();
                                this.plainTransactionManagerProvider.GetPlainTransactionManager().RollbackTransaction();

                                failedInterviewsCount++;
                            }

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
