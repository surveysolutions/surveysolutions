using System;
using SQLite;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.SharedKernels.Enumerator.Views
{
    public class InterviewView : IPlainStorageEntity
    {
        [PrimaryKey]
        public string Id { get; set; }
        public Guid InterviewId { get; set; }
        public string QuestionnaireId { get; set; }
        public Guid ResponsibleId { get; set; }
        public InterviewStatus Status { get; set; }
        public int? Assignment { get; set; }
        
        public string LastInterviewerOrSupervisorComment { get; set; }
        public bool CanBeDeleted { get; set; }
        public bool Census { get; set; }

        public DateTime? InterviewerAssignedDateTime { get; set; }
        public DateTime? StartedDateTime { get; set; }
        public DateTime? CompletedDateTime { get; set; }
        public DateTime? RejectedDateTime { get; set; }
        public DateTime? ApprovedDateTimeUtc { get; set; }
        public DateTime? FromHqSyncDateTime { get; set; }

        public byte[] AnswersOnPrefilledQuestions { get; set; }
        public byte[] GpsLocation { get; set; }
        
        public Guid? LocationQuestionId { get; set; }

        public double? LocationLatitude { get; set; }
        public double? LocationLongitude { get; set; }

        public string Language { get; set; }

        public string QuestionnaireTitle { get; set; }

        public string InterviewKey { get; set; }

        public DateTime? ReceivedByInterviewerAtUtc { get; set; }
        public string LastVisitedSectionId { get; set; }
        public ScreenType? LastVisitedScreenType { get; set; }
        public Guid? CalendarEventId { get; set; }
        public DateTime? CalendarEventLastUpdate { get; set; }
        public DateTimeOffset? CalendarEvent { get; set; }
        public string CalendarEventTimezoneId { get; set; }
        public string CalendarEventComment { get; set; }
        public InterviewMode Mode { get; set; }
    }
}
