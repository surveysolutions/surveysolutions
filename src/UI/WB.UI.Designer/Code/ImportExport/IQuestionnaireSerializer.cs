using WB.UI.Designer.Code.ImportExport.Models;

namespace WB.UI.Designer.Code.ImportExport
{
    public interface IQuestionnaireSerializer
    {
        string Serialize(Questionnaire questionnaire);
        Questionnaire Deserialize(string json);
    }
}