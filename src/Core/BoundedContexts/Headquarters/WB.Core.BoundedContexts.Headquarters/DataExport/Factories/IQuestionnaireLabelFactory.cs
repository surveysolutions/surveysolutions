using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views.Labels;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Factories
{
    [Obsolete("KP-11815")]
    internal interface IQuestionnaireLabelFactory
    {
        QuestionnaireLevelLabels[] CreateLabelsForQuestionnaire(QuestionnaireExportStructure structure);
    }
}
