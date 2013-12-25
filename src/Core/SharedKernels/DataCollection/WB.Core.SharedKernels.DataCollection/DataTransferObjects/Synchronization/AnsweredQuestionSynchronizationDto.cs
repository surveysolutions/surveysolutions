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
                if (questionPropagationVector == null)
                {
                    questionPropagationVector = RestoreFromPropagationVectorInOldIntFormat();
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

        private decimal[] RestoreFromPropagationVectorInOldIntFormat()
        {
            if (PropagationVector == null)
                return new decimal[0];
            return PropagationVector.Select(Convert.ToDecimal).ToArray();
        }
    }
}
