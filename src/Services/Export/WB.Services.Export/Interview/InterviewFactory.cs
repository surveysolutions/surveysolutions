using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Interview
{
    public class InterviewFactory : IInterviewFactory
    {
        private readonly ITenantApi<IHeadquartersApi> tenantApi;
        private DbConnectionSettings connectionSettings;

        public InterviewFactory(ITenantApi<IHeadquartersApi> tenantApi,
            IOptions<DbConnectionSettings> connectionSettings)
        {
            this.tenantApi = tenantApi;
            this.connectionSettings = connectionSettings.Value;
        }

        public async Task<List<InterviewEntity>> GetInterviewEntities(TenantInfo tenant, Guid[] interviewsId, QuestionnaireDocument questionnaire)
        {
            List<InterviewEntity> result = new List<InterviewEntity>();
            foreach (var group in questionnaire.GetAllStoredGroups())
            {
                List<string> columnsCollector = new List<string>();
                string BuildSelectColumns(string alias)
                {
                    foreach (var questionnaireEntity in group.Children)
                    {
                        if (questionnaireEntity is Question question)
                        {
                            columnsCollector.Add($" {alias}.\"{question.ColumnName}\" as \"{alias}_{question.ColumnName}\" ");
                        }

                        if (questionnaireEntity is Variable variable)
                        {
                            columnsCollector.Add($" {alias}.\"{variable.ColumnName}\" as \"{alias}_{variable.ColumnName}\" ");
                        }
                    }

                    return string.Join(", ", columnsCollector);
                }

                StringBuilder query = new StringBuilder($"select data.interview_id as data_interview_id, data.roster_vector as data_roster_vector");

                query.Append(BuildSelectColumns("data"));
                query.Append(", ");
                query.Append(BuildSelectColumns("enablement"));
                query.Append(", ");
                query.Append(BuildSelectColumns("validity"));

                query.AppendLine($" from ");
                query.AppendLine($"\"{tenant.Name}\".\"{group.TableName}\" data ");
                
                query.AppendFormat("    INNER JOIN \"{0}\".\"{1}_enablement\" enablement ON data.interview_id = enablement.interview_id{2}",
                    tenant.Name, group.TableName, Environment.NewLine);
                query.AppendFormat("    INNER JOIN \"{0}\".\"{1}_validity\" validity ON data.interview_id = validity.interview_id{2}",
                    tenant.Name, group.TableName, Environment.NewLine);
                query.AppendFormat(" WHERE data.interview_id = ANY(@ids)");

                using (var connection = new NpgsqlConnection(this.connectionSettings.DefaultConnection))
                {
                    await connection.OpenAsync();

                    var reader = await connection.ExecuteReaderAsync(query.ToString(), new {ids = interviewsId});

                    while (reader.Read())
                    {
                        foreach (var groupChild in group.Children)
                        {
                            if (groupChild is Question question)
                            {
                                result.Add(new InterviewEntity
                                {
                                    Identity = new Identity(question.PublicKey, (int[])reader["data_roster_vector"] ?? RosterVector.Empty),
                                    EntityType = EntityType.Question,
                                    InterviewId = (Guid)reader["data_interview_id"],
                                    AsObjectValue = reader[$"data_{question.ColumnName}"],
                                    InvalidValidations = (int[])reader[$"validity_{question.ColumnName}"],
                                    IsEnabled = (bool)reader[$"enablement_{question.ColumnName}"]
                                });
                            }
                            if (groupChild is Variable variable)
                            {
                                result.Add(new InterviewEntity
                                {
                                    Identity = new Identity(variable.PublicKey, (int[])reader["data_roster_vector"] ?? RosterVector.Empty),
                                    EntityType = EntityType.Variable,
                                    InterviewId = (Guid)reader["data_interview_id"],
                                    AsObjectValue = reader[$"data_{variable.ColumnName}"],
                                    IsEnabled = (bool)reader[$"enablement_{variable.ColumnName}"]
                                });
                            }
                        }
                    }

                  
                }
            }

            return result;
            //var api = this.tenantApi.For(tenant);
            //var entities = await api.GetInterviewBatchAsync(interviewsId);
            //return entities;
        }

        private InterviewEntity ReadSingleEntity(Guid entityId, )

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

            if (!entities.Any())
                return new List<MultimediaAnswer>();

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

        public async Task<List<AudioAuditInfo>> GetAudioAuditInfos(TenantInfo tenant,
            Guid[] interviewIds, CancellationToken cancellationToken)
        {
            var api = this.tenantApi.For(tenant);

            var audioInterviewViews = await api.GetAudioAuditInterviewsAsync(interviewIds);

            return audioInterviewViews.Select(ai =>
                new AudioAuditInfo
                {
                    InterviewId = ai.InterviewId,
                    FileNames = ai.Files.Select(f => f.FileName).ToArray(),
                }
            ).ToList();
        }
    }
}
