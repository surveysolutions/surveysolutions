using System;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Factories
{
    [Obsolete("KP-11815")]
    public interface IExportViewFactory
    {
        QuestionnaireExportStructure CreateQuestionnaireExportStructure(Guid id, long version);
        QuestionnaireExportStructure CreateQuestionnaireExportStructure(QuestionnaireIdentity id);
        InterviewDataExportView CreateInterviewDataExportView(QuestionnaireExportStructure exportStructure,
            InterviewData interview, IQuestionnaire questionnaire);
    }
}
