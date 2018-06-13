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

        public AnsweredQuestionSynchronizationDto(Guid id, decimal[] vector, object answer, CommentSynchronizationDto[] allComments, object protectedAnswer)
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
            ProtectedAnswer = protectedAnswer;
        }

        public Guid Id { get;  set; }

        public decimal[] QuestionRosterVector { get; set; }

        public object Answer { get;  set; }

        public object ProtectedAnswer { get;  set; }

        public CommentSynchronizationDto[] AllComments { get; set; }

        public bool IsAnswered()
        {
            return this.Answer != null;
        }

        public bool HasComments()
        {
            return this.AllComments != null && this.AllComments.Length > 0;
        }
    }
}
