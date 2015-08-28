using System;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Views.Interview;

namespace WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization
{
    public class AnsweredQuestionSynchronizationDto
    {
        public AnsweredQuestionSynchronizationDto()
        {
        }

        public AnsweredQuestionSynchronizationDto(Guid id, decimal[] vector, object answer, string comments)
        {
            Id = id;
            this.QuestionRosterVector = vector;

            var interviewTextListAnswers = answer as InterviewTextListAnswers;
            if (interviewTextListAnswers != null)
            {
                Answer =
                    interviewTextListAnswers.Answers.Select(a => new Tuple<decimal, string>(a.Value, a.Answer))
                        .ToArray();
            }
            else
            {
                Answer = answer;   
            }
           
            AllComments = new CommentSynchronizationDto[]{};
            Comments = comments;
        }

        public Guid Id { get;  set; }

        public decimal[] QuestionRosterVector { get; set; }

        public object Answer { get;  set; }
        public string Comments { get;  set; }

        public CommentSynchronizationDto[] AllComments { get; set; }

        public bool IsEmpty()
        {
            return this.Answer == null
                && string.IsNullOrWhiteSpace(this.Comments)
                && (this.AllComments == null || this.AllComments.Length == 0);
        }
    }
}
