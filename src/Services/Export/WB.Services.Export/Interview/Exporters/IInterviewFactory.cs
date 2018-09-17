using System;
using System.Collections;
using System.Collections.Generic;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services;

namespace WB.Services.Export.Interview.Exporters
{
    public interface IInterviewFactory
    {
        IEnumerable<InterviewEntity> GetInterviewEntities(IEnumerable<Guid> interviews);
        Dictionary<string, InterviewLevel> GetInterviewDataLevels(QuestionnaireDocument questionnaire, List<InterviewEntity> interviewEntities);
    }
}
