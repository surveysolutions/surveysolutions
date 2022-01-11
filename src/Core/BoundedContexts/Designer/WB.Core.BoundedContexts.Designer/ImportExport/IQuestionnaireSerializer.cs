using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.ImportExport.Models;

namespace WB.Core.BoundedContexts.Designer.ImportExport
{
    public interface IQuestionnaireSerializer
    {
        string Serialize(Questionnaire questionnaire);
        Questionnaire Deserialize(string json);
        string Serialize<T>(List<T> items);
        List<T> Deserialize<T>(string json);
    }
}