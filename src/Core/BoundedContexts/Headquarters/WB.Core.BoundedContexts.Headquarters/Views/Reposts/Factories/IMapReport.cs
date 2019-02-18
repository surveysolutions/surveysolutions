using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories
{
    public interface IMapReport
    {
        MapReportView Load(MapReportInputModel input);

        List<QuestionnaireBrowseItem> GetQuestionnaireIdentitiesWithGpsQuestions();
        List<string> GetGpsQuestionsByQuestionnaire(Guid questionnaireId, long? version);
    }
}
