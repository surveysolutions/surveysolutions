using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.CsvExport.Implementation.DoFiles
{
    internal interface IQuestionnaireLabelFactory
    {
        QuestionnaireLevelLabels[] CreateLabelsForQuestionnaire(QuestionnaireExportStructure structure);
    }
}
