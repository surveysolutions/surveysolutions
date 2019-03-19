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
using Supercluster;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
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
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> summaryRepository;
        private readonly IUnitOfWork sessionProvider;
        private readonly IPlainStorageAccessor<InterviewFlag> interviewFlagsStorage;

        public InterviewFactory(
            IQueryableReadSideRepositoryReader<InterviewSummary> summaryRepository,
            IUnitOfWork sessionProvider,
            IPlainStorageAccessor<InterviewFlag> interviewFlagsStorage)
        {
            this.summaryRepository = summaryRepository;
            this.sessionProvider = sessionProvider;
            this.interviewFlagsStorage = interviewFlagsStorage;
        }

        public Identity[] GetFlaggedQuestionIds(Guid interviewId)
            => this.interviewFlagsStorage.Query(x => x
                    .Where(y => y.InterviewId == interviewId)
                    .Select(y => new { y.EntityId, y.RosterVector }))
                .Select(x => Identity.Create(x.EntityId, RosterVector.Parse(x.RosterVector))).ToArray();

        public void SetFlagToQuestion(Guid interviewId, Identity questionIdentity, bool flagged)
        {
            var interview = summaryRepository.GetById(interviewId);

            if (interview == null)
                throw new InterviewException($"Interview {interviewId} not found.");

            ThrowIfInterviewApprovedByHq(interview);
            ThrowIfInterviewReceivedByInterviewer(interview);

            var rosterVector = questionIdentity.RosterVector.ToString().Trim('_');

            var flag = this.interviewFlagsStorage.Query(x => x.FirstOrDefault(y => y.InterviewId == interviewId &&
                                                                                   y.EntityId == questionIdentity.Id &&
                                                                                   y.RosterVector == rosterVector));
            if (flagged && flag == null)
            {
                this.interviewFlagsStorage.Store(new InterviewFlag
                {
                    InterviewId = interviewId,
                    EntityId = questionIdentity.Id,
                    RosterVector = rosterVector

                }, null);
            }
            else if (!flagged && flag != null)
                this.interviewFlagsStorage.Remove(flag);
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

            var interviewFlags = this.interviewFlagsStorage.Query(x => x.Where(y => y.InterviewId == interviewId));
            this.interviewFlagsStorage.Remove(interviewFlags);
        }

        private static InterviewStatus[] DisabledStatusesForGps =
        {
            InterviewStatus.ApprovedBySupervisor,
            InterviewStatus.ApprovedByHeadquarters
        };

        public InterviewGpsAnswer[] GetGpsAnswers(Guid questionnaireId, long? questionnaireVersion,
            string gpsQuestionVariableName, int? maxAnswersCount, Guid? supervisorId)
        {
            var gpsQuery = this.sessionProvider.Session
                .Query<InterviewGps>()
                .Join(this.sessionProvider.Session.Query<InterviewSummary>(),
                    gps => gps.InterviewId,
                    interview => interview.SummaryId,
                    (gps, interview) => new { gps, interview })
                .Join(this.sessionProvider.Session.Query<QuestionnaireCompositeItem>(),
                    interview_gps => new { interview_gps.interview.QuestionnaireIdentity, interview_gps.gps.QuestionId },
                    questionnaireItem => new { questionnaireItem.QuestionnaireIdentity, QuestionId = questionnaireItem.EntityId },
                    (interview_gps, questionnaireItem) => new { interview_gps, questionnaireItem })
                .Where(x => x.interview_gps.gps.IsEnabled &&
                            x.questionnaireItem.StatExportCaption == gpsQuestionVariableName &&
                            x.interview_gps.interview.QuestionnaireId == questionnaireId);

            if (questionnaireVersion.HasValue)
            {
                gpsQuery = gpsQuery
                    .Where(x => x.interview_gps.interview.QuestionnaireVersion == questionnaireVersion.Value);
            }

            if (supervisorId.HasValue)
            {
                gpsQuery = gpsQuery
                    .Where(x => x.interview_gps.interview.TeamLeadId == supervisorId.Value 
                                && !DisabledStatusesForGps.Contains(x.interview_gps.interview.Status));
            }

            var result = gpsQuery.Select(x => new InterviewGpsAnswer
            {
                InterviewId = Guid.Parse(x.interview_gps.gps.InterviewId),
                RosterVector = x.interview_gps.gps.RosterVector,
                Latitude = x.interview_gps.gps.Latitude,
                Longitude = x.interview_gps.gps.Longitude
            });

            if (maxAnswersCount.HasValue)
            {
                result = result.Take(maxAnswersCount.Value);
            }

            return result.ToArray();
        }

        public InterviewGpsAnswerWithTimeStamp[] GetGpsAnswersForInterviewer(Guid interviewerId) =>
            this.sessionProvider.Session
                .Query<InterviewGps>()
                .Join(this.sessionProvider.Session.Query<InterviewSummary>(),
                    gps => gps.InterviewId,
                    interview => interview.SummaryId,
                    (gps, interview) => new { gps, interview })
                .Join(this.sessionProvider.Session.Query<QuestionnaireCompositeItem>(),
                    interview_gps => new { interview_gps.interview.QuestionnaireIdentity, interview_gps.gps.QuestionId },
                    questionnaireItem => new { questionnaireItem.QuestionnaireIdentity, QuestionId = questionnaireItem.EntityId },
                    (interview_gps, questionnaireItem) => new { interview_gps, questionnaireItem })
                .Where(x => x.interview_gps.gps.IsEnabled && x.interview_gps.interview.ResponsibleId == interviewerId &&
                            x.questionnaireItem.QuestionScope == QuestionScope.Interviewer)
                .Select(x => new InterviewGpsAnswerWithTimeStamp
                {
                    EntityId = x.interview_gps.gps.QuestionId,
                    InterviewId = x.interview_gps.interview.InterviewId,
                    Latitude = x.interview_gps.gps.Latitude,
                    Longitude = x.interview_gps.gps.Longitude,
                    Timestamp = x.interview_gps.gps.Timestamp,
                    Idenifying = x.questionnaireItem.Featured == true,
                    Status = x.interview_gps.interview.Status
                })
                .ToArray();

        public bool HasAnyGpsAnswerForInterviewer(Guid interviewerId) =>
            this.sessionProvider.Session
                .Query<InterviewGps>()
                .Join(this.sessionProvider.Session.Query<InterviewSummary>(),
                    gps => gps.InterviewId,
                    interview => interview.SummaryId,
                    (gps, interview) => new { gps, interview })
                .Join(this.sessionProvider.Session.Query<QuestionnaireCompositeItem>(),
                    interview_gps => new { interview_gps.interview.QuestionnaireIdentity, interview_gps.gps.QuestionId },
                    questionnaireItem => new
                    { questionnaireItem.QuestionnaireIdentity, QuestionId = questionnaireItem.EntityId },
                    (interview_gps, questionnaireItem) => new { interview_gps, questionnaireItem })
                .Count(x => x.interview_gps.gps.IsEnabled && x.interview_gps.interview.ResponsibleId == interviewerId &&
                            x.questionnaireItem.QuestionScope == QuestionScope.Interviewer) > 0;

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

        private static void ThrowIfInterviewReceivedByInterviewer(InterviewSummary interview)
        {
            if (interview.ReceivedByInterviewer)
                throw new InterviewException($"Can't modify Interview {interview.InterviewId} on server, because it received by interviewer.");
        }

        private static void ThrowIfInterviewApprovedByHq(InterviewSummary interview)
        {
            if (interview.Status == InterviewStatus.ApprovedByHeadquarters)
                throw new InterviewException($"Interview was approved by Headquarters and cannot be edited. InterviewId: {interview.InterviewId}");
        }

        private static class Column
        {
            public const string InterviewId = "interviewid";
            public const string EntityId = "entityid";
            public const string RosterVector = "rostervector";
            public const string IsEnabled = "isenabled";
            public const string AsString = "asstring";
            public const string AsAudio = "asaudio";
            public const string QuestionnaireIdentity = "questionnaireidentity";
        }

        private static class Table
        {
            public const string Interviews = "readside.interviews";
            public const string InterviewsId = "readside.interviews_id";
            public const string InterviewsGeolocationAnswers = "readside.interview_geo_answers";
            public const string InterviewSummaries = "readside.interviewsummaries";
            public const string QuestionnaireEntities = "readside.questionnaire_entities";
            public const string InterviewsView = "readside.interviews_view";
        }
    }
}
