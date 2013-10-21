using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Core.Entities.SubEntities
{
    public static class NumericQuestionUtils
    {
        public static QuestionType GetQuestionTypeFromIsAutopropagatingParameter(bool isAutopropagating)
        {
            return isAutopropagating ? QuestionType.AutoPropagate : QuestionType.Numeric;
        }
    }
}
