using System.Collections.Generic;

namespace WB.Services.Export.Questionnaire
{
    public interface IValidatableQuestionnaireEntity : IQuestionnaireEntity
    {
        IList<ValidationCondition> ValidationConditions { get; set; }
    }

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

    public enum ValidationSeverity
    {
        Error = 0,
        Warning = 1
    }
}
