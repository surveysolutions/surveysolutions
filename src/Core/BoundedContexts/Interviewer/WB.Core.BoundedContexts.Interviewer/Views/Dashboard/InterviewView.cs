using System;
using SQLite;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class InterviewView : IPlainStorageEntity
    {
        [PrimaryKey]
        public string Id { get; set; }
        public Guid InterviewId { get; set; }
        public string QuestionnaireId { get; set; }
        public Guid ResponsibleId { get; set; }
        public InterviewStatus Status { get; set; }
        
        public string LastInterviewerOrSupervisorComment { get; set; }
        public bool CanBeDeleted { get; set; }
        public bool Census { get; set; }

        public DateTime? InterviewerAssignedDateTime { get; set; }
        public DateTime? StartedDateTime { get; set; }
        public DateTime? CompletedDateTime { get; set; }
        public DateTime? RejectedDateTime { get; set; }

        public byte[] AnswersOnPrefilledQuestions { get; protected set; }
        public byte[] GpsLocation { get; protected set; }

        //public InterviewAnswerOnPrefilledQuestionView[] AnswersOnPrefilledQuestions { get; set; }
        //public InterviewGpsLocationView GpsLocation { get; set; }

        public Guid? LocationQuestionId { get; set; }

        public double? LocationLatitude { get; set; }
        public double? LocationLongitude { get; set; }

        public string Language { get; set; }
    }
}