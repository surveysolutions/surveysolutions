using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
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
        private readonly DbConnectionSettings connectionSettings;

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
                using (var connection = new NpgsqlConnection(this.connectionSettings.DefaultConnection))
                {
                    await connection.OpenAsync();

                    var interviewsQuery = InterviewQueryBuilder.GetInterviewsQuery(tenant, @group);
                    var reader = await connection.ExecuteReaderAsync(interviewsQuery, new {ids = interviewsId});

                    while (reader.Read())
                    {
                        foreach (var groupChild in group.Children)
                        {
                            if (groupChild is Question question)
                            {
                                var identity = new Identity(question.PublicKey, group.IsInsideRoster ? (int[])reader["data__roster_vector"] : RosterVector.Empty);
                                var interviewEntity = new InterviewEntity
                                {
                                    Identity = identity,
                                    EntityType = EntityType.Question,
                                    InterviewId = (Guid)reader["data__interview_id"],
                                    InvalidValidations = reader[$"validity__{question.ColumnName}"] is DBNull ? Array.Empty<int>() : (int[])reader[$"validity__{question.ColumnName}"],
                                    IsEnabled = (bool)reader[$"enablement__{question.ColumnName}"]
                                };

                                var answer = reader[$"data__{question.ColumnName}"];
                                FillAnswerToQuestion(question, interviewEntity, answer is DBNull ? null : answer);
                                result.Add(interviewEntity);


                            }
                            else if (groupChild is Variable variable)
                            {
                                var identity = new Identity(variable.PublicKey, group.IsInsideRoster ? (int[])reader["data__roster_vector"] : RosterVector.Empty);
                                var interviewEntity = new InterviewEntity
                                {
                                    Identity = identity,
                                    EntityType = EntityType.Variable,
                                    InterviewId = (Guid)reader["data__interview_id"],
                                    IsEnabled = (bool)reader[$"enablement__{variable.ColumnName}"]
                                };
                                var val = reader[$"data__{variable.ColumnName}"];
                                FillAnswerToVariable(variable, interviewEntity, val is DBNull ? null : val);
                            }
                        }
                    }
                }
            }

            return result;
        }

        private void FillAnswerToVariable(Variable variable, InterviewEntity entity, object answer)
        {
            switch (variable.Type)
            {
                case VariableType.LongInteger:
                    entity.AsLong = (long?) answer;
                    break;
                case VariableType.Double:
                    entity.AsDouble = (double?) answer;
                    break;
                case VariableType.Boolean:
                    entity.AsBool = (bool?) answer;
                    break;
                case VariableType.DateTime:
                    entity.AsDateTime = (DateTime?) answer;
                    break;
                case VariableType.String:
                    entity.AsString = (string) answer;
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Variable {variable.Type} is not supported by export");
            }
        }

        private void FillAnswerToQuestion(Question question, InterviewEntity entity, object answer)
        {
            switch (question.QuestionType)
            {
                case QuestionType.MultyOption:
                    var multiOption = (MultyOptionsQuestion)question;

                    if (multiOption.YesNoView && answer != null)
                    {
                        entity.AsYesNo = JsonConvert.DeserializeObject<AnsweredYesNoOption[]>(answer.ToString());
                    }
                    else
                    {
                        entity.AsIntArray = (int[]) answer;
                    }
                    break;
                case QuestionType.Numeric:
                    entity.AsInt = (int?) answer;
                    break;
                case QuestionType.DateTime:
                    entity.AsDateTime = (DateTime?) answer;
                    break;
                case QuestionType.Multimedia:
                case QuestionType.QRBarcode:
                case QuestionType.Text:
                case QuestionType.SingleOption:
                    entity.AsString = (string) answer;
                    break;
                case QuestionType.GpsCoordinates:
                    entity.AsGps = answer != null ? JsonConvert.DeserializeObject<GeoPosition>(answer.ToString()) : null;
                    break;
                case QuestionType.Audio:
                    entity.AsAudio = answer != null ? JsonConvert.DeserializeObject<AudioAnswer>(answer.ToString()) : null;
                    break;
                case QuestionType.TextList:
                    entity.AsList = answer != null ? JsonConvert.DeserializeObject<InterviewTextListAnswer[]>(answer.ToString()) : null;
                    break;
                case QuestionType.Area:
                    entity.AsArea = answer != null ? JsonConvert.DeserializeObject<Area>(answer.ToString()) : null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Question {question.QuestionType} is not supported by export");
            }
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
