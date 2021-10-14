using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.UI.Designer.Code.ImportExport.Models
{
    public interface IQuestionnaireEntity
    {
        Guid PublicKey { get; }
        
        string VariableName { get; }
    }
}
