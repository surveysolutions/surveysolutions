using System;

namespace WB.Core.SharedKernels.SurveySolutions.Documents
{
    public class CriticalRule
    {
        public CriticalRule()
        {
        }

        public CriticalRule(Guid id, string? expression, string? message)
        {
            this.Id = id;
            this.Expression = expression;
            this.Message = message;
        }
        
        public Guid Id { get; set; }
        public string? Message { get; set; }
        public string? Expression { get; set; }
        public string? Description { get; set; }

        public CriticalRule Clone()
        {
            return new CriticalRule
            {
                Id = this.Id,
                Message = this.Message,
                Description = this.Description,
                Expression = this.Expression
            };
        }
    }
}
