using System;
using SQLite;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Views
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

        
        public InterviewAnswerOnPrefilledQuestionView[] AnswersOnPrefilledQuestions { get; set; }
        public InterviewGpsLocationView GpsLocation { get; set; }

        public string Language { get; set; }
    }
}