using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services;
using WB.Services.Export.Utils;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Interview
{
    public class InterviewFactory : IInterviewFactory
    {
        private readonly ITenantApi<IHeadquartersApi> tenantApi;

        public InterviewFactory(ITenantApi<IHeadquartersApi> tenantApi)
        {
            this.tenantApi = tenantApi;
        }

        public async Task<List<InterviewEntity>> GetInterviewEntities(TenantInfo tenant, Guid[] interviewsId)
        {
            var api = this.tenantApi.For(tenant);
            var entities = await api.GetInterviewBatchAsync(interviewsId);
            return entities;
        }

        public Dictionary<string, InterviewLevel> GetInterviewDataLevels(
            QuestionnaireDocument questionnaire,
            List<InterviewEntity> interviewEntities)
        {
            var levels = new Dictionary<string, InterviewLevel>();

            var entitiesByRosterScope = interviewEntities
                .ToLookup(x => new ValueVector<Guid>(questionnaire.GetRosterSizeSourcesForEntity(x.Identity.Id)));

            var interviewEntitiesLookup = interviewEntities.ToDictionary(i => i.Identity);

            foreach (var scopedEntities in entitiesByRosterScope)
            {
                var rosterScope = scopedEntities.Key;
                var entities = scopedEntities.ToList();
                var entitiesLevelLookup = entities.ToLookup(e => e.Identity.RosterVector);

                var rosterVectors = entities.Select(x => x.Identity?.RosterVector ?? RosterVector.Empty).Distinct()
                    .ToList();

                foreach (var rosterVector in rosterVectors)
                {
                    if (rosterVector.Length > 0)
                    {
                        var rosterIdentitiesInLevel = questionnaire.GetRostersInLevel(rosterScope).Select(x => new Identity(x, rosterVector));
                        var allGroupAreDisabled = CheckIfAllRostersAreDisabled(rosterIdentitiesInLevel, interviewEntitiesLookup);

                        if (allGroupAreDisabled)
                            continue;
                    }

                    var interviewLevel = new InterviewLevel
                    {
                        RosterVector = rosterVector,
                        RosterScope = rosterScope
                    };

                    foreach (var entity in entitiesLevelLookup[rosterVector])
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

                    var levelKey = InterviewLevel.GetLevelKeyName(rosterScope, rosterVector);
                    levels.Add(levelKey, interviewLevel);
                }
            }

            return levels;
        }

        

        private bool CheckIfAllRostersAreDisabled(IEnumerable<Identity> rosterIdentitiesInLevel, Dictionary<Identity, InterviewEntity> interviewEntities)
        {
            foreach (var rosterIdentity in rosterIdentitiesInLevel)
            {
                if (interviewEntities.TryGetValue(rosterIdentity, out var roster))
                {
                    if (roster.IsEnabled)
                        return false;
                }
                else
                {
                    return false;
                }
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
                    if (!a.IsEnabled) continue;

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
