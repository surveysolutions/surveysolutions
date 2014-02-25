using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    [Obsolete]
    public class NumericQuestionAnswered: QuestionAnswered
    {
        public decimal Answer { get; private set; }

        public NumericQuestionAnswered(Guid userId, Guid questionId, decimal[] propagationVector, DateTime answerTime, decimal answer)
            : base(userId, questionId, propagationVector, answerTime)
        {
            this.Answer = answer;
        }
    }
}
