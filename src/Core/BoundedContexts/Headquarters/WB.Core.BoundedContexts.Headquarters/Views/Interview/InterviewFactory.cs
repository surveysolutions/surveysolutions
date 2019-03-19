using Dapper;
using Main.Core.Entities.SubEntities;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Caching;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewFactory : IInterviewFactory
    {
        private readonly IUnitOfWork sessionProvider;

        public InterviewFactory(IUnitOfWork sessionProvider)
        {
            this.sessionProvider = sessionProvider;
        }

        public Identity[] GetFlaggedQuestionIds(Guid interviewId)
            => this.sessionProvider.Session.Query<InterviewFlag>()
                .Where(y => y.InterviewId == interviewId.ToString("N"))
                .Select(x => x.Identity).ToArray()
                .Select(Identity.Parse).ToArray();

        public void SetFlagToQuestion(Guid interviewId, Identity questionIdentity, bool flagged)
        {
            var sInterviewId = interviewId.ToString("N");
            var interview = this.sessionProvider.Session.Query<InterviewSummary>()
                .Where(x => x.SummaryId == sInterviewId)
                .Select(x => new {x.ReceivedByInterviewer, x.Status})
                .FirstOrDefault();

            if (interview == null)
                throw new InterviewException($"Interview {interviewId} not found.");
            if (interview.ReceivedByInterviewer)
                throw new InterviewException($"Can't modify Interview {interviewId} on server, because it received by interviewer.");
            if (interview.Status == InterviewStatus.ApprovedByHeadquarters)
                throw new InterviewException($"Interview was approved by Headquarters and cannot be edited. InterviewId: {interviewId}");

            var flag = this.sessionProvider.Session.Query<InterviewFlag>().FirstOrDefault(y =>
                y.InterviewId == sInterviewId && y.Identity == questionIdentity.ToString());

            if (flagged && flag == null)
            {
                this.sessionProvider.Session.Save(new InterviewFlag
                {
                    InterviewId = sInterviewId,
                    Identity = questionIdentity.ToString()
                });
            }
            else if (!flagged && flag != null)
                this.sessionProvider.Session.Delete(flag);
        }

        public void RemoveInterview(Guid interviewId)
        {
            var conn = sessionProvider.Session.Connection;
            conn.Execute($@"DELETE FROM {Table.Interviews} i
                    USING {Table.InterviewsId} s
                    WHERE i.{Column.InterviewId} = s.id AND s.{Column.InterviewId} = @InterviewId",
            new { InterviewId = interviewId });

            conn.Execute($@"DELETE FROM {Table.InterviewsId} i WHERE i.{Column.InterviewId} = @InterviewId",
                new { InterviewId = interviewId });
        }

        private static readonly InterviewStatus[] DisabledStatusesForGps =
        {
            InterviewStatus.ApprovedBySupervisor,
            InterviewStatus.ApprovedByHeadquarters
        };

        public InterviewGpsAnswer[] GetGpsAnswers(Guid questionnaireId, long? questionnaireVersion,
            string gpsQuestionVariableName, int? maxAnswersCount, Guid? supervisorId)
        {
            var gpsQuery = QueryGpsAnswers()
                .Where(x => x.Answer.IsEnabled &&
                            x.QuestionnaireItem.StatExportCaption == gpsQuestionVariableName &&
                            x.InterviewSummary.QuestionnaireId == questionnaireId);

            if (questionnaireVersion.HasValue)
            {
                gpsQuery = gpsQuery
                    .Where(x => x.InterviewSummary.QuestionnaireVersion == questionnaireVersion.Value);
            }

            if (supervisorId.HasValue)
            {
                gpsQuery = gpsQuery
                    .Where(x => x.InterviewSummary.TeamLeadId == supervisorId.Value 
                                && !DisabledStatusesForGps.Contains(x.InterviewSummary.Status));
            }

            var result = gpsQuery.Select(x => new InterviewGpsAnswer
            {
                InterviewId = Guid.Parse(x.Answer.InterviewId),
                RosterVector = x.Answer.RosterVector,
                Latitude = x.Answer.Latitude,
                Longitude = x.Answer.Longitude
            });

            if (maxAnswersCount.HasValue)
            {
                result = result.Take(maxAnswersCount.Value);
            }

            return result.ToArray();
        }
        
        public InterviewGpsAnswerWithTimeStamp[] GetGpsAnswersForInterviewer(Guid interviewerId) =>
            QueryGpsAnswers()
                .Where(x => x.Answer.IsEnabled 
                            && x.InterviewSummary.ResponsibleId == interviewerId 
                            && x.QuestionnaireItem.QuestionScope == QuestionScope.Interviewer)
                .Select(x => new InterviewGpsAnswerWithTimeStamp
                {
                    EntityId = x.Answer.QuestionId,
                    InterviewId = x.InterviewSummary.InterviewId,
                    Latitude = x.Answer.Latitude,
                    Longitude = x.Answer.Longitude,
                    Timestamp = x.Answer.Timestamp,
                    Idenifying = x.QuestionnaireItem.Featured == true,
                    Status = x.InterviewSummary.Status
                })
                .ToArray();

        private IQueryable<GpsAnswerQuery> QueryGpsAnswers()
        {
            return this.sessionProvider.Session
                .Query<InterviewGps>()
                .Join(this.sessionProvider.Session.Query<InterviewSummary>(),
                    gps => gps.InterviewId,
                    interview => interview.SummaryId,
                    (gps, interview) => new  { gps, interview})
                .Join(this.sessionProvider.Session.Query<QuestionnaireCompositeItem>(),
                    interview_gps => new { interview_gps.interview.QuestionnaireIdentity, interview_gps.gps.QuestionId },
                    questionnaireItem => new { questionnaireItem.QuestionnaireIdentity, QuestionId = questionnaireItem.EntityId },
                    (interview_gps, questionnaireItem) => new GpsAnswerQuery
                    {
                        InterviewSummary = interview_gps.interview,
                        Answer = interview_gps.gps,
                        QuestionnaireItem = questionnaireItem
                    });
        }

        public bool HasAnyGpsAnswerForInterviewer(Guid interviewerId) =>
            QueryGpsAnswers()
                .Any(x => x.Answer.IsEnabled 
                            && x.InterviewSummary.ResponsibleId == interviewerId 
                            && x.QuestionnaireItem.QuestionScope == QuestionScope.Interviewer);

        public void Save(InterviewState state)
        {
            var rows = GetInterviewEntities(state.Id);

            var perEntity = rows.ToDictionary(r => InterviewStateIdentity.Create(r.Identity));

            foreach (var removed in state.Removed)
            {
                perEntity.Remove(removed);
            }

            void Upsert<T>(Dictionary<InterviewStateIdentity, T> valueSource, Action<InterviewEntity, T> action)
            {
                foreach (var item in valueSource)
                {
                    if (perEntity.TryGetValue(item.Key, out var entity))
                    {
                        action(entity, item.Value);
                    }
                    else
                    {
                        entity = new InterviewEntity
                        {
                            InterviewId = state.Id,
                            IsEnabled = true,
                            Identity = Identity.Create(item.Key.Id, item.Key.RosterVector)
                        };
                        action(entity, item.Value);
                        perEntity.Add(item.Key, entity);
                    }
                }
            }

            Upsert(state.ReadOnly.ToDictionary(r => r, v => true), (e, v) => e.IsReadonly = v);
            Upsert(state.Validity, (e, v) => e.InvalidValidations = v.Validations);
            Upsert(state.Warnings, (e, v) => e.WarningValidations = v.Validations);

            Upsert(state.Answers, (entity, answer) =>
            {
                entity.AsString = answer.AsString;
                entity.AsInt = answer.AsInt;
                entity.AsLong = answer.AsLong;
                entity.AsDouble = answer.AsDouble;
                entity.AsDateTime = answer.AsDatetime;
                entity.AsList = answer.AsList;
                entity.AsIntArray = answer.AsIntArray;
                entity.AsIntMatrix = answer.AsIntMatrix;
                entity.AsBool = answer.AsBool;
                entity.AsYesNo = answer.AsYesNo;
                entity.AsAudio = answer.AsAudio;
                entity.AsArea = answer.AsArea;

                entity.IsEnabled = true; //if the answer was changed we assume that question is enabled
                                         //and disabling would come from state.Enablement
            });

            //order is important
            //overrides with saved value if it was set to true in value setting state
            Upsert(state.Enablement, (entity, value) => entity.IsEnabled = value);

            SaveInterviewStateItem(state.Id, perEntity.Values);
        }

        private Dictionary<Guid, int> GetQuestionnaireEntities(string questionnaireIdentity)
        {
            var cache = MemoryCache.Default;
            var cacheKey = "Questionnaire_" + (questionnaireIdentity ?? "<null>");
            var cacheValue = cache.Get(cacheKey);

            if (cacheValue is Dictionary<Guid, int> cached) return cached;

            var entities = this.sessionProvider.Session.Connection
                .Query<(int id, Guid entityId)>(
                    @"SELECT id, entityid FROM readside.questionnaire_entities where questionnaireidentity = @questionnaireIdentity",
                    new { questionnaireIdentity });

            var entitiesMap = entities.ToDictionary(q => q.entityId, q => q.id);
            cache.Add(cacheKey, entitiesMap, new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(10) });

            return entitiesMap;
        }

        private (int id, string questionnaireId) GetInterviewId(Guid interviewId)
        {
            var conn = sessionProvider.Session.Connection;
            var id = interviewId.FormatGuid();
            return conn.QuerySingleOrDefault<(int id, string questionnaireId)>(
                // https://stackoverflow.com/a/42217872/41483
                // will return id for new @interviewid or select existing
                $@"WITH input_rows(interviewid) AS (values (uuid '{id}'))
                , ins AS (
                   INSERT INTO readside.interviews_id (interviewid) 
                   SELECT * FROM input_rows
                   ON CONFLICT (interviewid) DO NOTHING
                   RETURNING id )
             select ids.id, s.questionnaireidentity
             from (
                SELECT id FROM ins 
                UNION  ALL
                SELECT c.id FROM input_rows
                JOIN readside.interviews_id c USING (interviewid) 
             ) as ids
                left join readside.interviewsummaries s on s.interviewid = '{id}'");
        }

        private void SaveInterviewStateItem(Guid interviewId, IEnumerable<InterviewEntity> stateItems)
        {
            var conn = sessionProvider.Session.Connection;

            var interview = GetInterviewId(interviewId);

            var entityMap = GetQuestionnaireEntities(interview.questionnaireId);

            conn.Execute($@"delete from {Table.Interviews} where {Column.InterviewId} = {interview.id}");

            var npgConnection = conn as NpgsqlConnection ?? throw new NotSupportedException("Cannot import over non Postgres connection");

            using (var importer = npgConnection.BeginBinaryImport(@"copy 
                readside.interviews (
                    interviewid, entityid, rostervector, isenabled, isreadonly, invalidvalidations, warnings, asstring, asint, aslong, 
                    asdouble, asdatetime, aslist, asintarray, asintmatrix, asbool, asyesno, asaudio, asarea 
                ) 
                from stdin (format binary)"))
            {
                foreach (var item in stateItems)
                {
                    importer.StartRow();
                    Write(interview.id);
                    Write(entityMap[item.Identity.Id]);
                    Write(item.Identity.RosterVector.ToString().Trim('_'));
                    Write(item.IsEnabled);
                    Write(item.IsReadonly);
                    Write(item.InvalidValidations?.Length > 0 ? string.Join("-", item.InvalidValidations) : null);
                    Write(item.WarningValidations?.Length > 0 ? string.Join("-", item.WarningValidations) : null);
                    Write(item.AsString);
                    if (item.AsInt != null) importer.Write(item.AsInt.Value); else importer.WriteNull();
                    if (item.AsLong != null) importer.Write(item.AsLong.Value); else importer.WriteNull();
                    if (item.AsDouble != null) importer.Write(item.AsDouble.Value); else importer.WriteNull();
                    if (item.AsDateTime != null) importer.Write(item.AsDateTime.Value); else importer.WriteNull();
                    WriteJson(item.AsList);
                    Write(item.AsIntArray);
                    WriteJson(item.AsIntMatrix);
                    if (item.AsBool != null) importer.Write(item.AsBool.Value); else importer.WriteNull();
                    WriteJson(item.AsYesNo);
                    WriteJson(item.AsAudio);
                    WriteJson(item.AsArea);

                    void Write<T>(T value)
                    {
                        if (value == null) importer.WriteNull(); else importer.Write(value);
                    }

                    void WriteJson<T>(T value)
                    {
                        if (value == null) importer.WriteNull();
                        else importer.Write(JsonConvert.SerializeObject(value), NpgsqlDbType.Jsonb);
                    }
                }

                importer.Complete();
            }

            conn.Execute($"DO $$ BEGIN PERFORM readside.update_report_table_data({interview.id}); END $$;");
        }

        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        public IEnumerable<InterviewEntity> GetInterviewEntities(IEnumerable<Guid> interviews, Guid[] entityIds = null)
        {
            var connection = sessionProvider.Session.Connection;

            var ids = string.Join(",", interviews.Select(i => "'" + i.ToString() + "'"));

            // for some reason Postgres decide that it's good to sequence scan whole interviews table
            // following line will ensure that Postgres will not do that
            connection.Execute("set enable_seqscan=false");

            string queryBase =
                $@"SELECT interviewid, entityid, rostervector, isenabled, 
                         isreadonly, invalidvalidations, warnings, asstring, asint,
                         aslong, asdouble, asdatetime, aslist, asintarray, asintmatrix, 
                         asbool, asyesno, asaudio, asarea, entity_type as EntityType 
                         from {Table.InterviewsView} ";

            var query = queryBase + $" where {Column.InterviewId} in ({ids})";

            if (entityIds != null && entityIds.Length > 0)
            {
                var entityIdCondition = string.Join(",", entityIds.Select(i => "'" + i.ToString() + "'"));

                query += $" and entityid in ({entityIdCondition})";
            }

            var queryResult = connection.Query<InterviewEntityDto>(query, commandTimeout: 0, buffered: false);

            foreach (var result in queryResult)
            {
                var entity = new InterviewEntity();

                entity.InterviewId = result.InterviewId;
                entity.Identity = new Identity(result.EntityId, result.RosterVector.ParseMinusDelimitedIntArray() ?? RosterVector.Empty);

                entity.IsEnabled = result.IsEnabled;
                entity.IsReadonly = result.IsReadonly;
                entity.InvalidValidations = result.InvalidValidations.ParseMinusDelimitedIntArray();
                entity.WarningValidations = result.Warnings.ParseMinusDelimitedIntArray();
                entity.AsString = result.AsString;
                entity.AsInt = result.AsInt;
                entity.AsLong = result.AsLong;
                entity.AsDouble = result.AsDouble;
                entity.AsDateTime = result.AsDateTime;
                entity.AsIntArray = result.AsIntArray;
                entity.AsBool = result.AsBool;

                entity.AsList = Deserialize<InterviewTextListAnswer[]>(result.AsList);
                entity.AsIntMatrix = Deserialize<int[][]>(result.AsIntMatrix);
                entity.AsYesNo = Deserialize<AnsweredYesNoOption[]>(result.AsYesNo);
                entity.AsAudio = Deserialize<AudioAnswer>(result.AsAudio);
                entity.AsArea = Deserialize<Area>(result.AsArea);
                entity.EntityType = result.EntityType;

                T Deserialize<T>(string value) where T : class
                {
                    if (string.IsNullOrWhiteSpace(value)) return null;
                    return JsonConvert.DeserializeObject<T>(value);
                }

                yield return entity;
            }
        }

        public List<InterviewEntity> GetInterviewEntities(Guid interviews)
        {
            return GetInterviewEntities(new[] { interviews }).ToList();
        }

        private static class Column
        {
            public const string InterviewId = "interviewid";
        }

        private static class Table
        {
            public const string Interviews = "readside.interviews";
            public const string InterviewsId = "readside.interviews_id";
            public const string InterviewsView = "readside.interviews_view";
        }

        private class GpsAnswerQuery
        {
            public InterviewGps Answer { get; set; }
            public InterviewSummary InterviewSummary { get; set; }
            public QuestionnaireCompositeItem QuestionnaireItem { get; set; }
        }
    }
}
