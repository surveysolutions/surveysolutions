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

        public ValidationCondition Clone()
        {
            return new ValidationCondition(this.Expression, this.Message);
        }
    }
}