using System;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.Core.BoundedContexts.Designer.ImportExport.Models
{
    public class ValidationCondition
    {
        public string Expression { get; set; } = String.Empty; 
        public string Message { get; set; } = String.Empty;

        public ValidationSeverity Severity { set; get; }
    }
}
