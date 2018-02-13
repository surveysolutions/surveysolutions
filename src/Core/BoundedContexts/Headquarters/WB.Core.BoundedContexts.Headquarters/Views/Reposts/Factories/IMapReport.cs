using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories
{
    public interface IMapReport
    {
        MapReportView Load(MapReportInputModel input);

        List<QuestionnaireBrowseItem> GetQuestionnaireIdentitiesWithPoints();
        List<string> GetGpsQuestionsByQuestionnaire(QuestionnaireIdentity questionnaireIdentity);
    }
}