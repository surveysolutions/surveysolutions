using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class NumericIntegerQuestionAnswered : QuestionAnswered
    {
        public int Answer { get; private set; }

        public NumericIntegerQuestionAnswered(Guid userId, Guid questionId, decimal[] propagationVector, DateTime answerTime, int answer)
            : base(userId, questionId, propagationVector, answerTime)
        {
            this.Answer = answer;
        }
    }
}
