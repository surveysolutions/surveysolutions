using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.DataCollection.ValueObjects.Questionnaire
{
    public class QuestionIdAndVariableName
    {
        public QuestionIdAndVariableName(Guid id, string variableName)
        {
            this.Id = id;
            this.VariableName = variableName;
        }

        public Guid Id { get; private set; }
        public string VariableName { get; private set; }
    }
}
