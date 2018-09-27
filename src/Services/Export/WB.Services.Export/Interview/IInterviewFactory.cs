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
using WB.Services.Export.Utils;

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

        public Dictionary<string, InterviewLevel> GetInterviewDataLevels(
             QuestionnaireDocument questionnaire,
             List<InterviewEntity> interviewEntities)
        {
            var levels = new Dictionary<string, InterviewLevel>();

            var entitiesByRosterScope = interviewEntities
                .ToLookup(x => new ValueVector<Guid>(questionnaire.GetRosterSizeSourcesForEntity(x.Identity.Id)));

            foreach (var scopedEntities in entitiesByRosterScope)
            {
                var rosterScope = scopedEntities.Key;
                var entities = scopedEntities.ToList();

                var rosterVectors = entities.Select(x => x.Identity?.RosterVector ?? RosterVector.Empty).Distinct()
                    .ToList();

                foreach (var rosterVector in rosterVectors)
                {
                    if (rosterVector.Length > 0)
                    {
                        var rosterIdentitiesInLevel = questionnaire.GetRostersInLevel(rosterScope).Select(x => new Identity(x, rosterVector));
                        var allGroupAreDisabled = CheckIfAllRostersAreDisabled(rosterIdentitiesInLevel, interviewEntities);

                        if (allGroupAreDisabled)
                            continue;
                    }

                    var interviewLevel = new InterviewLevel
                    {
                        RosterVector = rosterVector,
                        RosterScope = rosterScope
                    };

                    foreach (var entity in entities.Where(x => x.Identity.RosterVector == rosterVector))
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

                    var keyParts = rosterScope.Select(x => x.FormatGuid()).ToList();
                    if (rosterVector.Length == 0)
                        keyParts.Add("#");
                    else
                    {
                        keyParts.AddRange(rosterVector.Select(x => x.ToString()));
                    }

                    var levelKey = string.Join("-", keyParts);
                    levels.Add(levelKey, interviewLevel);
                }
            }

            return levels;
        }

        private bool CheckIfAllRostersAreDisabled(IEnumerable<Identity> rosterIdentitiesInLevel, List<InterviewEntity> interviewEntities)
        {
            foreach (var rosterIdentity in rosterIdentitiesInLevel)
            {
                var roster = interviewEntities.FirstOrDefault(x => x.Identity.Equals(rosterIdentity));
                if (roster == null)
                {
                    // no records in DB that roster was disabled, because disablement event hasn't been raised 
                    return false;
                }

                if (roster.IsEnabled)
                    return false;
            }

            return true;
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

            IEnumerable<MultimediaAnswer> ToMultimediaAnswer()
            {
                foreach (var a in answerLines)
                {
                    var type = entities[a.Identity.Id];

                    if (type == typeof(MultimediaQuestion))
                    {
                        if (a.AsString == null) continue;

                        yield return new MultimediaAnswer
                        {
                            Answer = a.AsString,
                            InterviewId = a.InterviewId,
                            Type = MultimediaType.Image
                        };

                        continue;
                    }
                    
                    if (type == typeof(AudioQuestion))
                    {
                        if (a.AsAudio == null) continue;

                        yield return new MultimediaAnswer
                        {
                            Answer = a.AsAudio.FileName,
                            InterviewId = a.InterviewId,
                            Type = MultimediaType.Audio
                        };

                        continue;
                    }

                    throw new NotSupportedException();
                }
            }

            return ToMultimediaAnswer().ToList();
        }
    }
}
