using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services;
using WB.Services.Export.Tenant;

namespace WB.Services.Export.Interview
{
    public interface IInterviewFactory
    {
        Task<IEnumerable<InterviewEntity>> GetInterviewEntities(TenantInfo tenant, Guid interviewId);
        Dictionary<string, InterviewLevel> GetInterviewDataLevels(QuestionnaireDocument questionnaire, List<InterviewEntity> interviewEntities);
        InterviewStringAnswer[] GetMultimediaAnswersByQuestionnaire(QuestionnaireId settingsQuestionnaireId);
        InterviewStringAnswer[] GetAudioAnswersByQuestionnaire(QuestionnaireId settingsQuestionnaireId);
    }

    internal class InterviewFactory : IInterviewFactory
    {
        private readonly ITenantApi<IHeadquartersApi> headquartersApi;

        public InterviewFactory(ITenantApi<IHeadquartersApi> headquartersApi)
        {
            this.headquartersApi = headquartersApi;
        }

        public async Task<IEnumerable<InterviewEntity>> GetInterviewEntities(TenantInfo tenant, Guid interviewId)
        {
            var api = this.headquartersApi.For(tenant);
            var entities = await api.GetInterviewAsync(interviewId);
            return entities;
        }

        public Dictionary<string, InterviewLevel> GetInterviewDataLevels(QuestionnaireDocument questionnaire, List<InterviewEntity> interviewEntities)
        {
            var interviewLevels = interviewEntities
                .GroupBy(x => x.Identity?.RosterVector ?? RosterVector.Empty)
                .Select(x => ToInterviewLevel(x.Key, x.ToArray(), questionnaire));

            var interviewDataLevels = interviewLevels.ToDictionary(k => CreateLevelIdFromPropagationVector(k.RosterVector), v => v);
            return interviewDataLevels;
        }

        public InterviewStringAnswer[] GetMultimediaAnswersByQuestionnaire(QuestionnaireId settingsQuestionnaireId)
        {
            throw new NotImplementedException();
        }

        public InterviewStringAnswer[] GetAudioAnswersByQuestionnaire(QuestionnaireId settingsQuestionnaireId)
        {
            throw new NotImplementedException();
        }

        public static string CreateLevelIdFromPropagationVector(RosterVector vector)
        {
            if (vector.Length == 0)
                return "#";
            return vector.ToString();
        }

        private InterviewLevel ToInterviewLevel(RosterVector rosterVector, InterviewEntity[] interviewDbEntities, QuestionnaireDocument questionnaire)
        {
            Dictionary<ValueVector<Guid>, int?> scopeVectors = new Dictionary<ValueVector<Guid>, int?>();
            if (rosterVector.Length > 0)
            {
                // too slow
                scopeVectors = interviewDbEntities
                    .Select(x => questionnaire.GetRosterSizeSourcesForEntity(x.Identity.Id))
                    .Select(x => new ValueVector<Guid>(x))
                    .Distinct()
                    .ToDictionary(x => x, x => (int?)0);
            }
            else
            {
                scopeVectors.Add(new ValueVector<Guid>(), 0);
            }

            var interviewLevel = new InterviewLevel
            {
                ScopeVectors = scopeVectors,
                RosterVector = rosterVector
            };

            foreach (var entity in interviewDbEntities)
            {
                switch (entity.EntityType)
                {
                    case EntityType.Question:
                        interviewLevel.QuestionsSearchCache.Add(entity.Identity.Id, entity);
                        break;
                    case EntityType.Variable:
                        interviewLevel.Variables.Add(entity.Identity.Id, entity.AsObject());
                        if (entity.IsEnabled == false)
                            interviewLevel.DisabledVariables.Add(entity.Identity.Id);
                        break;
                }
            }

            return interviewLevel;
        }
    }
}
