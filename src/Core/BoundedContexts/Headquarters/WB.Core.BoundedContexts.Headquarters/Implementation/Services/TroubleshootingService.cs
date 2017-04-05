using System;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
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

        public string GetMissingDataReason(Guid? interviewId, string interviewKey)
        {
            InterviewSummary interview = interviewId.HasValue 
                ? this.interviewSummaryReader.GetById(interviewId.Value) 
                : this.interviewSummaryReader.Query(q => q.Where(x => x.Key == interviewKey).Take(1).ToList()).SingleOrDefault();

            if (interview == null)
                return "The interview was not found";

            BrokenInterviewPackage lastBrokenPackage = brokenPackagesFactory.GetLastInterviewBrokenPackage(interview.InterviewId);

            InterviewLog interviewLog = this.syncLogFactory.GetInterviewLog(interview.InterviewId, interview.ResponsibleId);
            
            var lastUploadedInterviewIsBroken = interviewLog.LastUploadInterviewDate <= lastBrokenPackage?.IncomingDate;

            if (!statusesEligibleForSyncronization.Contains(interview.Status))
            {
                if (lastUploadedInterviewIsBroken)
                {
                    return "Contact support to recover interview";
                }
                return "The interview is out of interviewer's responsibility";
            }

            var questionnaire = questionnaireFactory.GetById(new QuestionnaireIdentity(interview.QuestionnaireId, interview.QuestionnaireVersion));

            if (interview.IsDeleted || interview.Status == InterviewStatus.Deleted)
            {
                if (questionnaire.IsDeleted)
                    return " The interview was deleted with its template and cannot be restored";

                return "The interview was deleted";
            }

            if (interview.ReceivedByInterviewer == false)
            {
                if (lastUploadedInterviewIsBroken && interviewLog.InterviewWasNotDownloadedAfterItWasUploaded)
                {
                    return "Contact support to recover interview";
                }
                return $"The interview was not recieved by responsible interviewer {interview.ResponsibleName}";
            }

            if (lastBrokenPackage!=null && 
                lastBrokenPackage.ExceptionType == InterviewDomainExceptionType.OtherUserIsResponsible.ToString() && 
                lastUploadedInterviewIsBroken)
            {
                return "The interview was re-assigned. Recieved oackage was not applied";
            }

            if (lastBrokenPackage != null &&
                lastBrokenPackage.ExceptionType == "Unexpected" && 
                lastUploadedInterviewIsBroken)
            {
                return "Survey solutions team should be contacted.";
            }
            
            if (interviewLog.IsInterviewOnDevice && interviewLog.WasDeviceLinkedAfterInterviewWasDownloaded)
                return "The interview can be deleted by interviewer if it is marked as received and after that, there is record that tablet was cleared. Get back to interviewer";

            if (interviewLog.IsInterviewOnDevice)
            {
                if (interviewLog.InterviewerChangedDeviceBetweenDownloads)
                {
                    return "The interview was not uploaded by an interviewer yet, but user changed devices, so make sure started interviews were not deleted during this process";
                }
                return "The interview was not uploaded by an interviewer yet";
            }

            return "Unknown";
        }
    }
}
