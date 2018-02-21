using System.Xml.Schema;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.Core.SharedKernels.QuestionnaireEntities
{
    public class ValidationCondition
    {
        public ValidationCondition()
        {
        }

        public ValidationCondition(string expression, string message)
        {
            this.Expression = expression;
            this.Message = message;
        }

        public string Expression { get; set; } 
        public string Message { get; set; }

        public ValidationSeverity Severity { set; get; }

        public ValidationCondition Clone()
        {
            return new ValidationCondition(this.Expression, this.Message)
            {
                Severity = this.Severity
            };
        }
    }
}