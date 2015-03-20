using System;
using System.Linq;
using System.Threading.Tasks;
using Ncqrs;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Code
{
    internal class RevalidateInterviewsAdministrationService : IRevalidateInterviewsAdministrationService
    {
        private static bool areInterviewsBeingRevalidatingNow = false;
        private static bool shouldStopInterviewsRevalidating = false;

        private static readonly object RebuildAllViewsLockObject = new object();
        private static string statusMessage;

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
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewsReader)
        {
            this.logger = logger;
            this.commandService = commandService;
            this.interviewsReader = interviewsReader;
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

                var interviews = this.interviewsReader.Query(_ => _.Where(interview =>
                    interview.HasErrors == true && interview.IsDeleted == false &&
                        (interview.Status == InterviewStatus.Completed ||
                            interview.Status == InterviewStatus.RejectedBySupervisor ||
                            interview.Status == InterviewStatus.ApprovedBySupervisor ||
                            interview.Status == InterviewStatus.ApprovedByHeadquarters ||
                            interview.Status == InterviewStatus.RejectedByHeadquarters)).ToList());

                UpdateStatusMessage("Determining count of interview to be revalidated.");

                ThrowIfShouldStopViewsRebuilding();

                int allInterviewsCount = interviews.Count();

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

                        var interviewItemIds = interviews.Skip(i * pageSize).Take(pageSize).ToList();

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
