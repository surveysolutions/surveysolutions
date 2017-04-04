using System;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    public class TroubleshootingService : ITroubleshootingService
    {
        private readonly IPlainStorageAccessor<BrokenInterviewPackage> brokenInterviewPackagesReader;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader;
        private readonly IPlainStorageAccessor<SynchronizationLogItem> syncLogAccessor;
        private readonly IQuestionnaireBrowseViewFactory questionnaireFactory;

        public TroubleshootingService(
            IPlainStorageAccessor<BrokenInterviewPackage> brokenInterviewPackagesReader, 
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader, 
            IPlainStorageAccessor<SynchronizationLogItem> syncLogAccessor, 
            IQuestionnaireBrowseViewFactory questionnaireFactory)
        {
            this.brokenInterviewPackagesReader = brokenInterviewPackagesReader;
            this.interviewSummaryReader = interviewSummaryReader;
            this.syncLogAccessor = syncLogAccessor;
            this.questionnaireFactory = questionnaireFactory;
        }

        public string GetMissingDataReason(Guid? interviewId, string interviewKey)
        {
            InterviewSummary interview = interviewId.HasValue 
                ? this.interviewSummaryReader.GetById(interviewId.Value) 
                : this.interviewSummaryReader.Query(q => q.Where(x => x.Key == interviewKey).Take(1).ToList()).SingleOrDefault();

            if (interview == null)
                return "The interview was not found";
            
            if (interview.Status != InterviewStatus.RejectedBySupervisor && 
                interview.Status != InterviewStatus.InterviewerAssigned && 
                interview.Status != InterviewStatus.Deleted)
            {
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
                return $"The interview was not recieved by responsible interviewer {interview.ResponsibleName}";

            var lastBrokenPackage = this.brokenInterviewPackagesReader
              .Query(queryable => queryable.Where(x => x.InterviewId == interviewId)
              .OrderByDescending(x => x.IncomingDate)
              .Take(1)
              .ToList())
              .SingleOrDefault();

            if (lastBrokenPackage!=null && 
                lastBrokenPackage.ExceptionType == InterviewDomainExceptionType.OtherUserIsResponsible.ToString())
            {
                return "The interview was re-assigned. Recieved oackage was not applied";
            }
            
            var lastDownloadInterviewDates = syncLogAccessor.Query(queryable => queryable
                .Where(x => x.Type == SynchronizationLogType.GetInterview)
                .Where(x => x.Log.Contains(interviewId.ToString()))
                .OrderByDescending(x => x.LogDate)
                .Select(x => x.LogDate)
                .ToList());

            DateTime firstDownloadInterviewDate = lastDownloadInterviewDates.Last();
            DateTime lastDownloadInterviewDate = lastDownloadInterviewDates.First();

            DateTime? lastUploadInterviewDate = syncLogAccessor.Query(queryable => queryable
                .Where(x => x.Type == SynchronizationLogType.PostInterview)
                .Where(x => x.Log.Contains(interviewId.ToString()))
                .OrderByDescending(x => x.LogDate)
                .Take(1)
                .ToList()).SingleOrDefault()?.LogDate;

            if (lastBrokenPackage != null &&
                lastBrokenPackage.ExceptionType == "Unexpected" && lastUploadInterviewDate < lastBrokenPackage.IncomingDate)
            {
                return "Survey solutions team should be contacted.";
            }

            bool isInterviewOnDevice = !lastUploadInterviewDate.HasValue ||
                                       lastDownloadInterviewDate > lastUploadInterviewDate;


            DateTime? lastLinkDate = syncLogAccessor.Query(queryable => queryable
                .Where(x => x.Type == SynchronizationLogType.LinkToDevice && x.InterviewerId == interview.ResponsibleId)
                .OrderByDescending(x => x.LogDate)
                .Take(1)
                .ToList()).SingleOrDefault()?.LogDate;

            bool wasDeviceLinkedAfterInterviewWasDownloaded = lastLinkDate > lastDownloadInterviewDate;
            if (isInterviewOnDevice && wasDeviceLinkedAfterInterviewWasDownloaded)
                return "The interview can be deleted by interviewer if it is marked as received and after that, there is record that tablet was cleared. Get back to interviewer";

            if (isInterviewOnDevice)
            {
                if (firstDownloadInterviewDate <= lastLinkDate && lastLinkDate <= lastDownloadInterviewDate)
                {
                    return "The interview was not uploaded by an interviewer yet, but user changed devices, so make sure started interviews were not deleted during this process";
                }
                return "The interview was not uploaded by an interviewer yet";
            }

            return "Unknown";
        }
    }
}
