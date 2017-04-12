using System;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Troubleshooting.Views;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.BrokenInterviewPackages;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
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
                return string.Format(TroubleshootingMessages.NoData_NotFound, interviewId?.FormatGuid() ?? interviewKey);

            var questionnaire = this.questionnaireFactory.GetById(new QuestionnaireIdentity(interview.QuestionnaireId, interview.QuestionnaireVersion));

            if (interview.IsDeleted || interview.Status == InterviewStatus.Deleted)
            {
                if (questionnaire.IsDeleted)
                    return TroubleshootingMessages.NoData_QuestionnaireDeleted.FormatString(interview.Key);

                return TroubleshootingMessages.NoData_InterviewDeleted.FormatString(interview.Key);
            }

            BrokenInterviewPackage lastBrokenPackage = this.brokenPackagesFactory.GetLastInterviewBrokenPackage(interview.InterviewId);

            InterviewSyncLogSummary interviewSyncLogSummary = this.syncLogFactory.GetInterviewLog(interview.InterviewId, interview.ResponsibleId);
            
            var lastUploadedInterviewIsBroken = interviewSyncLogSummary.LastUploadInterviewDate <= lastBrokenPackage?.IncomingDate;

            if (lastUploadedInterviewIsBroken)
            {
                if (lastBrokenPackage?.ExceptionType == InterviewDomainExceptionType.OtherUserIsResponsible.ToString())
                {
                    return TroubleshootingMessages.NoData_InterviewWasReassigned.FormatString(interview.Key);
                }

                return TroubleshootingMessages.NoData_ContactSupport.FormatString(interview.Key);
            }

            var interviewIsOnServer = !this.statusesEligibleForSyncronization.Contains(interview.Status);
            if (interviewIsOnServer)
            {
                return TroubleshootingMessages.NoData_NoIssuesInterviewOnServer.FormatString(interview.Key);
            }

            if (interview.ReceivedByInterviewer == false)
            {
                return TroubleshootingMessages.NoData_InterviewWasNotReceived.FormatString(interview.Key, interview.ResponsibleName);
            }
            
            if (interviewSyncLogSummary.IsInterviewOnDevice)
            {
                if (interviewSyncLogSummary.WasDeviceLinkedAfterInterviewWasDownloaded)
                {
                    return TroubleshootingMessages.NoData_InterviewerChangedDevice.FormatString(interview.Key);
                }

                if (interviewSyncLogSummary.InterviewerChangedDeviceBetweenDownloads)
                {
                    return TroubleshootingMessages.NoData_InterviewerChangedDevice.FormatString(interview.Key);
                }
                return TroubleshootingMessages.NoData_InterveiwWasNotUploadedYet.FormatString(interview.Key);
            }

            return TroubleshootingMessages.NoData_ContactSupportWithMoreDetails;
        }
    }
}
