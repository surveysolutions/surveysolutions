using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.ImportExport.Models;

namespace WB.Core.BoundedContexts.Designer.ImportExport
{
    public interface IQuestionnaireSerializer
    {
        string Serialize(Questionnaire questionnaire);
        Questionnaire Deserialize(string json);
        string SerializeTranslations(List<TranslationItem> translationItems);
        List<TranslationItem> DeserializeTranslations(string json);
    }
}