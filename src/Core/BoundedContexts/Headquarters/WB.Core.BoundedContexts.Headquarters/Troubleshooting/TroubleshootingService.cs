using System;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.BrokenInterviewPackages;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Troubleshooting
{
    public class TroubleshootingService : ITroubleshootingService
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader;
        private readonly IQuestionnaireBrowseViewFactory questionnaireFactory;
        private readonly IInterviewLogSummaryReader syncLogFactory;
        private readonly IBrokenInterviewPackagesViewFactory brokenPackagesFactory;

        public TroubleshootingService(
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader, 
            IQuestionnaireBrowseViewFactory questionnaireFactory,
            IInterviewLogSummaryReader syncLogFactory, 
            IBrokenInterviewPackagesViewFactory brokenPackagesFactory)
        {
            this.interviewSummaryReader = interviewSummaryReader;
            this.questionnaireFactory = questionnaireFactory;
            this.syncLogFactory = syncLogFactory;
            this.brokenPackagesFactory = brokenPackagesFactory;
        }

        private readonly InterviewStatus[] statusesEligibleForSyncronization = 
        {
            InterviewStatus.RejectedBySupervisor,
            InterviewStatus.InterviewerAssigned,
            InterviewStatus.Deleted
        };

        public string GetCensusInterviewsMissingReason(string questionnaireId, Guid? interviewerId, DateTime changedFrom,
            DateTime changedTo)
        {
            BrokenInterviewPackagesView brokenItems = this.brokenPackagesFactory.GetFilteredItems(new BrokenInterviewPackageFilter
            {
                QuestionnaireIdentity = questionnaireId,
                FromProcessingDateTime = changedFrom,
                ToProcessingDateTime = changedTo,
                ResponsibleId = interviewerId
            });

            return brokenItems.TotalCount == 0
                ? TroubleshootingMessages.MissingCensusInterviews_NoBrokenPackages_Message
                : TroubleshootingMessages.MissingCensusInterviews_SomeBrokenPackages_Message;
        }

        public string GetMissingDataReason(Guid? interviewId, string interviewKey)
        {
            InterviewSummary interview = interviewId.HasValue 
                ? this.interviewSummaryReader.GetById(interviewId.Value) 
                : this.interviewSummaryReader.Query(q => q.SingleOrDefault(x => x.Key == interviewKey));

            if (interview == null)
                return TroubleshootingMessages.NoData_NotFound;

            var questionnaire = this.questionnaireFactory.GetById(new QuestionnaireIdentity(interview.QuestionnaireId, interview.QuestionnaireVersion));

            if (interview.IsDeleted || interview.Status == InterviewStatus.Deleted)
            {
                if (questionnaire.IsDeleted)
                    return TroubleshootingMessages.NoData_QuestionnaireDeleted;

                return TroubleshootingMessages.NoData_InterviewDeleted;
            }

            BrokenInterviewPackage lastBrokenPackage = this.brokenPackagesFactory.GetLastInterviewBrokenPackage(interview.InterviewId);

            InterviewLog interviewLog = this.syncLogFactory.GetInterviewLog(interview.InterviewId, interview.ResponsibleId);
            
            var lastUploadedInterviewIsBroken = interviewLog.LastUploadInterviewDate <= lastBrokenPackage?.IncomingDate;

            if (lastUploadedInterviewIsBroken)
            {
                if (lastBrokenPackage?.ExceptionType == InterviewDomainExceptionType.OtherUserIsResponsible.ToString())
                {
                    return TroubleshootingMessages.NoData_InterviewWasReassigned;
                }

                return TroubleshootingMessages.NoData_ContactSupport;
            }

            var interviewIsOnServer = !this.statusesEligibleForSyncronization.Contains(interview.Status);
            if (interviewIsOnServer)
            {
                return TroubleshootingMessages.NoData_NoIssuesInterviewOnServer;
            }

            if (interview.ReceivedByInterviewer == false)
            {
                return string.Format(TroubleshootingMessages.NoData_InterviewWasNotReceived, interview.ResponsibleName);
            }
            
            if (interviewLog.IsInterviewOnDevice)
            {
                if (interviewLog.WasDeviceLinkedAfterInterviewWasDownloaded)
                {
                    return TroubleshootingMessages.NoData_InterviewerChangedDevice;
                }

                if (interviewLog.InterviewerChangedDeviceBetweenDownloads)
                {
                    return TroubleshootingMessages.NoData_InterviewerChangedDevice;
                }
                return TroubleshootingMessages.NoData_InterveiwWasNotUploadedYet;
            }

            return TroubleshootingMessages.NoData_ContactSupportWithMoreDetails;
        }
    }
}
