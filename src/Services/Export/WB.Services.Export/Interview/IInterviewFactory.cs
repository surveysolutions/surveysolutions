using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Interview
{
    public interface IInterviewFactory
    {
        Task<List<InterviewEntity>> GetInterviewEntities(TenantInfo tenant, Guid[] interviewsId);
        Dictionary<string, InterviewLevel> GetInterviewDataLevels(QuestionnaireDocument questionnaire, List<InterviewEntity> interviewEntities);
        Task<List<MultimediaAnswer>> GetMultimediaAnswersByQuestionnaire(TenantInfo tenant,
            QuestionnaireDocument questionnaire, Guid[] interviewIds, CancellationToken cancellationToken);
    }

    public struct MultimediaAnswer
    {
        public Guid InterviewId { get; set; }
        public string Answer { get; set; }
        public MultimediaType Type { get; set; }
    }

    public enum MultimediaType
    {
        Image, Audio
    }
}
