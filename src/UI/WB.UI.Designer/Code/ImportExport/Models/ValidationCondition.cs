using System;
using System.Xml.Schema;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.UI.Designer.Code.ImportExport.Models
{
    public class ValidationCondition
    {
        public string Expression { get; set; } = String.Empty; 
        public string Message { get; set; } = String.Empty;

        public ValidationSeverity Severity { set; get; }
    }
}
