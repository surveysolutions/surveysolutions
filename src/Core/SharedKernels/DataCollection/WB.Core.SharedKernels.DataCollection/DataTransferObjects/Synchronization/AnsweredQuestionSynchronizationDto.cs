using System;
using System.Linq;

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
            QuestionPropagationVector = vector;
            Answer = answer;
            Comments = comments;
        }

        public Guid Id { get;  set; }

        public decimal[] QuestionPropagationVector {
            get
            {
                if (questionPropagationVector == null && PropagationVector != null)
                {
                    questionPropagationVector = PropagationVector.Select(v => (decimal) v).ToArray();
                }
                return questionPropagationVector;
            }
            set { questionPropagationVector = value; }
        }

        private decimal[] questionPropagationVector;
        
        [Obsolete("please use QuestionPropagationVector instead")]
        public int[] PropagationVector { get; set; }

        public object Answer { get;  set; }
        public string Comments { get;  set; }
    }
}
