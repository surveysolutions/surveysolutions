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

        public MapReportView GetReport(List<PositionPoint> gpsAnswers, int zoom, int clientMapWidth, double south,
            double north, double east, double west);

        List<QuestionnaireBrowseItem> GetQuestionnaireIdentitiesWithGpsQuestions();
        List<string> GetGpsQuestionsByQuestionnaire(Guid questionnaireId, long? version);
    }
}
