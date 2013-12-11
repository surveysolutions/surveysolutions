using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class MultipleOptionsLinkedQuestionAnswered : QuestionAnswered
    {
        public decimal[][] SelectedPropagationVectors { get; private set; }

        public MultipleOptionsLinkedQuestionAnswered(Guid userId, Guid questionId, decimal[] propagationVector, DateTime answerTime, decimal[][] selectedPropagationVectors)
            : base(userId, questionId, propagationVector, answerTime)
        {
            this.SelectedPropagationVectors = selectedPropagationVectors;
        }
    }
}
