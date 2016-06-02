using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views.Labels;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Factories
{
    internal interface IQuestionnaireLabelFactory
    {
        QuestionnaireLevelLabels[] CreateLabelsForQuestionnaire(QuestionnaireExportStructure structure);
    }
}