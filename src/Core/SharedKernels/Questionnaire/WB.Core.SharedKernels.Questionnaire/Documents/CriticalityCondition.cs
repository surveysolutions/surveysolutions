using System;

namespace WB.Core.SharedKernels.SurveySolutions.Documents
{
    public class CriticalityCondition
    {
        public CriticalityCondition()
        {
        }

        public CriticalityCondition(Guid id, string? expression, string? message)
        {
            this.Id = id;
            this.Expression = expression;
            this.Message = message;
        }
        
        public Guid Id { get; set; }
        public string? Message { get; set; }
        public string? Expression { get; set; }
        public string? Description { get; set; }

        public CriticalityCondition Clone()
        {
            return new CriticalityCondition
            {
                Message = this.Message,
                Description = this.Description,
                Expression = this.Expression
            };
        }
    }
}
