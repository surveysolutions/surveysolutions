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

        public AnsweredQuestionSynchronizationDto(Guid id, decimal[] vector, object answer, CommentSynchronizationDto[] allComments)
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
           
            AllComments = allComments;
        }

        public Guid Id { get;  set; }

        public decimal[] QuestionRosterVector { get; set; }

        public object Answer { get;  set; }

        public CommentSynchronizationDto[] AllComments { get; set; }

        public bool IsEmpty()
        {
            return this.Answer == null && (this.AllComments == null || this.AllComments.Length == 0);
        }
    }
}
