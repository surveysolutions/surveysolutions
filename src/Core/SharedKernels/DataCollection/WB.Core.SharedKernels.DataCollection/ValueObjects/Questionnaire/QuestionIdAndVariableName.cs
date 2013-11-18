using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.DataCollection.ValueObjects.Questionnaire
{
    public struct QuestionIdAndVariableName
    {
        public QuestionIdAndVariableName(Guid id, string variableName)
        {
            Id = id;
            VariableName = variableName;
        }

        public readonly Guid Id;
        public readonly string VariableName;
    }
}
