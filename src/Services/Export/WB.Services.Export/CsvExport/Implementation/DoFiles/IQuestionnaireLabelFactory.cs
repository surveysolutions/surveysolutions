using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.CsvExport.Implementation.DoFiles
{
    public interface IQuestionnaireLabelFactory
    {
        QuestionnaireLevelLabels[] CreateLabelsForQuestionnaire(QuestionnaireExportStructure structure);
    }
}
