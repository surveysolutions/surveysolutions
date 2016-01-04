using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views.Labels;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Factories
{
    internal interface IQuestionnaireLabelFactory
    {
        QuestionnaireLabels CreateLabelsForQuestionnaireLevel(QuestionnaireExportStructure structure, ValueVector<Guid> levelRosterVector);
    }
}