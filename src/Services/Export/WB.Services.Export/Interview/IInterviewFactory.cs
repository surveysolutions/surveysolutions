using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Questionnaire.Services;
using WB.Services.Export.Services;
using WB.Services.Export.Tenant;

namespace WB.Services.Export.Interview
{
    public interface IInterviewFactory
    {
        Task<IEnumerable<InterviewEntity>> GetInterviewEntities(TenantInfo tenant, Guid interviewId);
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

    public class InterviewFactory : IInterviewFactory
    {
        private readonly ITenantApi<IHeadquartersApi> tenantApi;
        private readonly IQuestionnaireStorage questionnaireStorage;

        public InterviewFactory(ITenantApi<IHeadquartersApi> tenantApi, IQuestionnaireStorage questionnaireStorage)
        {
            this.tenantApi = tenantApi;
            this.questionnaireStorage = questionnaireStorage;
        }

        public async Task<IEnumerable<InterviewEntity>> GetInterviewEntities(TenantInfo tenant, Guid interviewId)
        {
            var api = this.tenantApi.For(tenant);
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
        
        public async Task<List<MultimediaAnswer>> GetMultimediaAnswersByQuestionnaire(TenantInfo tenant,
            QuestionnaireDocument questionnaire, Guid[] interviewIds, CancellationToken cancellationToken)
        {
            var entities = questionnaire.Children.TreeToEnumerable(c => c.Children)
                .Where(c => c is MultimediaQuestion || c is AudioQuestion)
                .Select(c => (c.PublicKey, c.GetType()))
                .ToDictionary(c => c.Item1, c => c.Item2);

            var api = this.tenantApi.For(tenant);
            
            var answerLines = await api.GetInterviewBatchAsync(interviewIds, entities.Keys.ToArray());

            return answerLines.Select(a =>
            {
                var type = entities[a.Identity.Id];

                if (type == typeof(MultimediaQuestion))
                {
                    return new MultimediaAnswer
                    {
                        Answer = a.AsString,
                        InterviewId = a.InterviewId,
                        Type = MultimediaType.Image
                    };
                }

                if (type == typeof(AudioQuestion))
                {
                    return new MultimediaAnswer
                    {
                        Answer = a.AsAudio.FileName,
                        InterviewId = a.InterviewId,
                        Type = MultimediaType.Audio
                    };
                }

                throw new NotSupportedException();

            }).ToList();
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
