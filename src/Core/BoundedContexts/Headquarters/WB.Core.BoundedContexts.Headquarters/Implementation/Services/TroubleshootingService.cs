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

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    public class TroubleshootingService : ITroubleshootingService
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader;
        private readonly IQuestionnaireBrowseViewFactory questionnaireFactory;
        private readonly ISynchronizationLogViewFactory syncLogFactory;
        private readonly IBrokenInterviewPackagesViewFactory brokenPackagesFactory;

        public TroubleshootingService(
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader, 
            IQuestionnaireBrowseViewFactory questionnaireFactory, 
            ISynchronizationLogViewFactory syncLogFactory, 
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
                ? Troubleshooting.MissingCensusInterviews_NoBrokenPackages_Message
                : Troubleshooting.MissingCensusInterviews_SomeBrokenPackages_Message;
        }

        public string GetMissingDataReason(Guid? interviewId, string interviewKey)
        {
            InterviewSummary interview = interviewId.HasValue 
                ? this.interviewSummaryReader.GetById(interviewId.Value) 
                : this.interviewSummaryReader.Query(q => q.SingleOrDefault(x => x.Key == interviewKey));

            if (interview == null)
                return Troubleshooting.NoData_NotFound;

            var questionnaire = questionnaireFactory.GetById(new QuestionnaireIdentity(interview.QuestionnaireId, interview.QuestionnaireVersion));

            if (interview.IsDeleted || interview.Status == InterviewStatus.Deleted)
            {
                if (questionnaire.IsDeleted)
                    return Troubleshooting.NoData_QuestionnaireDeleted;

                return Troubleshooting.NoData_InterviewDeleted;
            }

            BrokenInterviewPackage lastBrokenPackage = brokenPackagesFactory.GetLastInterviewBrokenPackage(interview.InterviewId);

            InterviewLog interviewLog = this.syncLogFactory.GetInterviewLog(interview.InterviewId, interview.ResponsibleId);
            
            var lastUploadedInterviewIsBroken = interviewLog.LastUploadInterviewDate <= lastBrokenPackage?.IncomingDate;

            if (lastUploadedInterviewIsBroken)
            {
                if (lastBrokenPackage?.ExceptionType == InterviewDomainExceptionType.OtherUserIsResponsible.ToString())
                {
                    return Troubleshooting.NoData_InterviewWasReassigned;
                }

                return Troubleshooting.NoData_ContactSupport;
            }

            var interviewIsOnServer = !this.statusesEligibleForSyncronization.Contains(interview.Status);
            if (interviewIsOnServer)
            {
                return Troubleshooting.NoData_NoIssuesInterviewOnServer;
            }

            if (interview.ReceivedByInterviewer == false)
            {
                return string.Format(Troubleshooting.NoData_InterviewWasNotReceived, interview.ResponsibleName);
            }
            
            if (interviewLog.IsInterviewOnDevice)
            {
                if (interviewLog.WasDeviceLinkedAfterInterviewWasDownloaded)
                {
                    return Troubleshooting.NoData_InterviewerChangedDevice;
                }

                if (interviewLog.InterviewerChangedDeviceBetweenDownloads)
                {
                    return Troubleshooting.NoData_InterviewerChangedDevice;
                }
                return Troubleshooting.NoData_InterveiwWasNotUploadedYet;
            }

            return Troubleshooting.NoData_ContactSupportWithMoreDetails;
        }
    }
}
