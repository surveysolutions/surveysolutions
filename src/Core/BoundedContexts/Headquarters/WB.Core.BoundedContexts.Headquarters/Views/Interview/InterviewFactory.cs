using Dapper;
using Main.Core.Entities.SubEntities;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewFactory : IInterviewFactory
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> summaryRepository;
        private readonly ISessionProvider sessionProvider;
        private readonly IPlainStorageAccessor<QuestionnaireCompositeItem> questionnaireItems;

        public InterviewFactory(
            IQueryableReadSideRepositoryReader<InterviewSummary> summaryRepository,
            ISessionProvider sessionProvider,
            IPlainStorageAccessor<QuestionnaireCompositeItem> questionnaireItems)
        {
            this.summaryRepository = summaryRepository;
            this.sessionProvider = sessionProvider;
            this.questionnaireItems = questionnaireItems;
        }

        public Identity[] GetQuestionsWithFlagBySectionId(QuestionnaireIdentity questionnaireId, Guid interviewId,
            Identity sectionId)
        {
            return sessionProvider.GetSession().Connection.Query<(Guid entityId, string rosterVector)>(
                    $@"SELECT {Column.EntityId} as Id, {Column.RosterVector}
                       FROM {Table.InterviewsView}
                       WHERE {Column.InterviewId} = @InterviewId
                        AND {Column.HasFlag} = true
                        AND parentid = @sectionId
                        AND {Column.RosterVector} = @RosterVector",
                    new
                    {
                        InterviewId = interviewId,
                        RosterVector = sectionId.RosterVector.ToString().Trim('_'),
                        SectionId = sectionId.Id
                    })
                .Select(x => Identity.Create(x.entityId, RosterVector.Parse(x.rosterVector)))
                .ToArray();
        }

        public Identity[] GetFlaggedQuestionIds(Guid interviewId)
            => sessionProvider.GetSession().Connection.Query<(Guid entityId, string rosterVector)>(
                    $"SELECT {Column.EntityId}, {Column.RosterVector} " +
                    $"FROM {Table.InterviewsView} WHERE {Column.InterviewId} = @InterviewId AND {Column.HasFlag} = true",
                    new { InterviewId = interviewId })
                .Select(x => Identity.Create(x.entityId, RosterVector.Parse(x.rosterVector)))
                .ToArray();

        public void SetFlagToQuestion(Guid interviewId, Identity questionIdentity, bool flagged)
        {
            var interview = summaryRepository.GetById(interviewId);

            if (interview == null)
                throw new InterviewException($"Interview {interviewId} not found.");

            ThrowIfInterviewApprovedByHq(interview);
            ThrowIfInterviewReceivedByInterviewer(interview);

            var entityMap = GetQuestionnaireEntities(interview.QuestionnaireIdentity);

            var row = new
            {
                interview.Id,
                entityId = entityMap[questionIdentity.Id],
                rosterVector = questionIdentity.RosterVector.ToString().Trim('_'),
                hasFlag = flagged
            };

            var flaggedRows = sessionProvider.GetSession().Connection
                .Query<(int interviewId, int entityId, string rosterVector, bool hasFlag)>
                ($"select interviewid, entityid, rostervector, hasflag from {Table.Interviews} " +
                 $"where interviewid = @id and entityid = @entityid and rostervector = @rostervector", row).ToList();

            switch (flaggedRows.Count)
            {
                // do not add new row with false value
                case 0 when flagged == false:
                    return;
                case 0:
                    sessionProvider.GetSession().Connection.Execute(
                        $@"INSERT INTO {Table.Interviews} (interviewid, entityid, rostervector, isenabled, hasflag)
                       VALUES(@id, @entityid, @rostervector, true, @hasFlag)", row);
                    break;
                default:
                    sessionProvider.GetSession().Connection.Execute(
                        $@"UPDATE {Table.Interviews} SET hasflag=@hasFlag where interviewid=@id and entityid=@entityid and rostervector=@rostervector", row);
                    break;
            }
        }

        public void RemoveInterview(Guid interviewId)
            => sessionProvider.GetSession().Connection.Execute(
                $@"DELETE FROM {Table.Interviews} i
                    USING {Table.InterviewSummaries} s
                    WHERE i.{Column.InterviewId} = s.id AND s.{Column.InterviewId} = @InterviewId",
                new { InterviewId = interviewId });

        public InterviewStringAnswer[] GetMultimediaAnswersByQuestionnaire(QuestionnaireIdentity questionnaireIdentity)
        {
            return sessionProvider.GetSession().Connection
                .Query<InterviewStringAnswer>(
                    $@"select s.{Column.InterviewId}, i.{Column.AsString} as answer
                       from {Table.Interviews} i
                       join {Table.InterviewSummaries} s on s.id = i.{Column.InterviewId} 
                            and s.{Column.QuestionnaireIdentity} = @questionnaireIdentity
                       join {Table.QuestionnaireEntities} q on q.id = i.{Column.EntityId}
                        where i.{Column.AsString} is not null and q.question_type = @questionType", new
                    {
                        questionType = QuestionType.Multimedia,
                        questionnaireIdentity = questionnaireIdentity.ToString()
                    })
                .ToArray();
        }

        public InterviewStringAnswer[] GetAudioAnswersByQuestionnaire(QuestionnaireIdentity questionnaireIdentity)
            => sessionProvider.GetSession().Connection.Query<InterviewStringAnswer>(
                $"SELECT i.{Column.InterviewId}, i.{Column.AsAudio}->>'{nameof(AudioAnswer.FileName)}' as Answer " +
                $"FROM readside.interviewsummaries s INNER JOIN {Table.InterviewsView} i ON(s.interviewid = i.{Column.InterviewId}) " +
                $"WHERE {Column.QuestionnaireIdentity} = '{questionnaireIdentity}' " +
                $"AND {Column.AsAudio} IS NOT NULL " +
                $"AND {Column.IsEnabled} = true").ToArray();


        public InterviewGpsAnswer[] GetGpsAnswers(QuestionnaireIdentity questionnaireIdentity,
            Guid gpsQuestionId, int maxAnswersCount, double northEastCornerLatitude,
            double southWestCornerLatitude, double northEastCornerLongtitude, double southWestCornerLongtitude,
            Guid? supervisorId)
        {
            var result = sessionProvider.GetSession().Connection.Query<InterviewGpsAnswer>(
                $@"select interviewid, latitude, longitude
                        from (
	                        select s.interviewid, (i.asgps ->> 'Latitude')::float8 as latitude, (i.asgps ->> 'Longitude')::float8 as longitude
                            from readside.interviews i
                            join {Table.QuestionnaireEntities} q on q.id = i.entityid
                            join readside.interviewsummaries s on s.id = i.interviewid
                            where 
                                i.asgps is not null and s.questionnaireidentity = @Questionnaire and q.entityid = @QuestionId
                                and (@supervisorId is null OR s.teamleadid = @supervisorId)
	                        ) as q
                        where latitude > @SouthWestCornerLatitude and latitude < @NorthEastCornerLatitude
                            and longitude > @SouthWestCornerLongtitude
                                {(northEastCornerLongtitude >= southWestCornerLongtitude ? "AND" : "OR")} 
                                longitude < @NorthEastCornerLongtitude
                        limit @MaxCount",
                new
                {
                    supervisorId,
                    Questionnaire = questionnaireIdentity.ToString(),
                    QuestionId = gpsQuestionId,
                    MaxCount = maxAnswersCount,
                    SouthWestCornerLatitude = southWestCornerLatitude,
                    NorthEastCornerLatitude = northEastCornerLatitude,
                    SouthWestCornerLongtitude = southWestCornerLongtitude,
                    NorthEastCornerLongtitude = northEastCornerLongtitude
                })
            .ToArray();
            return result;
        }

        //public InterviewGpsAnswer[] GetGpsAnswers(
        //    => sessionProvider.GetSession().Connection.Query<Guid>(
        //        $"SELECT {Column.EntityId} " +
        //        $"FROM readside.interviewsummaries s INNER JOIN {Table.InterviewsView} i ON(s.interviewid = i.{Column.InterviewId}) " +
        //        $"WHERE {Column.QuestionnaireIdentity} = '{questionnaireIdentity}' " +
        //        $"AND {Column.AsGpsColumn} is not null " +
        //        $"GROUP BY {Column.EntityId} ").ToArray();
        //    QuestionnaireIdentity questionnaireIdentity,
        //    Guid gpsQuestionId, int maxAnswersCount, double northEastCornerLatitude,
        //    double southWestCornerLatitude, double northEastCornerLongtitude, double southWestCornerLongtitude,
        //    Guid? supervisorId)
        //    => sessionProvider.GetSession().Connection.Query<string>(
        //        $@"select distinct {Column.QuestionnaireIdentity} from {Table.InterviewSummaries}").ToArray();
        //public InterviewGpsAnswer[] GetGpsAnswersByQuestionIdAndQuestionnaire(
        //    QuestionnaireIdentity questionnaireIdentity,
        //    => sessionProvider.GetSession().Connection.Query<InterviewGpsAnswer>(
        //            $@"select interviewid, latitude, longitude
        //                from (
        //                 select s.interviewid, (i.asgps ->> 'Latitude')::float8 as latitude, (i.asgps ->> 'Longitude')::float8 as longitude
        //                    from readside.interviews i
        //            $"WHERE {(supervisorId.HasValue ? $"s.teamleadid = '{supervisorId}' AND s.status NOT IN ({(int)InterviewStatus.ApprovedBySupervisor}, {(int)InterviewStatus.ApprovedByHeadquarters}) AND " : "")}" +
        //            $"questionnaireidentity = @Questionnaire " +
        //                    join {Table.QuestionnaireEntities} q on q.id = i.entityid
        //                    join readside.interviewsummaries s on s.id = i.interviewid
        //                        where i.asgps is not null and s.questionnaireidentity = @Questionnaire and q.entityid = @QuestionId            
        //                 ) as q
        //                where  latitude > @SouthWestCornerLatitude and latitude < @NorthEastCornerLatitude
        //                    and longitude > @SouthWestCornerLongtitude
        //                        {(northEastCornerLongtitude >= southWestCornerLongtitude ? "AND" : "OR")} 
        //                        longitude < @NorthEastCornerLongtitude
        //                limit @MaxCount",
        //            new
        //            {
        //                Questionnaire = questionnaireIdentity.ToString(),
        //                QuestionId = gpsQuestionId,
        //                MaxCount = maxAnswersCount,
        //                SouthWestCornerLatitude = southWestCornerLatitude,
        //                NorthEastCornerLatitude = northEastCornerLatitude,
        //                SouthWestCornerLongtitude = southWestCornerLongtitude,
        //                NorthEastCornerLongtitude = northEastCornerLongtitude
        //            })
        //        .ToArray();

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

            Upsert(state.Enablement, (entity, value) => entity.IsEnabled = value);
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
                entity.AsGps = answer.AsGps;
                entity.AsBool = answer.AsBool;
                entity.AsYesNo = answer.AsYesNo;
                entity.AsAudio = answer.AsAudio;
                entity.AsArea = answer.AsArea;
            });

            SaveInterviewStateItem(state.Id, perEntity.Values.Where(IsNeeded).ToList());

            bool IsNeeded(InterviewEntity entity)
            {
                return entity.EntityType != EntityType.Section;
            }
        }

        private Dictionary<Guid, int> GetQuestionnaireEntities(string questionnaireIdentity)
        {
            var cache = MemoryCache.Default;
            var cacheKey = "Questionnaire_" + questionnaireIdentity;
            var cacheValue = cache.Get(cacheKey);

            if (cacheValue is Dictionary<Guid, int> cached) return cached;

            var entities = questionnaireItems.Query(q => q
                .Where(w => w.QuestionnaireIdentity == questionnaireIdentity)
                .Select(w => new { w.Id, w.EntityId })
                .ToList());

            var entitiesMap = entities.ToDictionary(q => q.EntityId, q => q.Id);
            cache.Add(cacheKey, entitiesMap, new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(10) });

            return entitiesMap;
        }

        private void SaveInterviewStateItem(Guid interviewId, IEnumerable<InterviewEntity> stateItems)
        {
            var conn = sessionProvider.GetSession().Connection;

            var summary = summaryRepository.GetById(interviewId)
                ?? throw new ArgumentException($@"Cannot find interview summary for interviewd: {interviewId}", nameof(interviewId));

            var entityMap = GetQuestionnaireEntities(summary.QuestionnaireIdentity);

            conn.Execute($@"delete from {Table.Interviews} where {Column.InterviewId} = {summary.Id}");

            var npgConnection = conn as NpgsqlConnection ?? throw new NotSupportedException("Cannot import over non Postgres connection");

            using (var importer = npgConnection.BeginBinaryImport(@"copy 
                readside.interviews (
                    interviewid, entityid, rostervector, isenabled, isreadonly, invalidvalidations, warnings, asstring, asint, aslong, 
                    asdouble, asdatetime, aslist, asintarray, asintmatrix, asgps, asbool, asyesno, asaudio, asarea, hasflag 
                ) 
                from stdin (format binary)"))
            {
                foreach (var item in stateItems)
                {
                    importer.StartRow();
                    Write(summary.Id);
                    Write(entityMap[item.Identity.Id]);
                    Write(item.Identity.RosterVector.ToString().Trim('_'));
                    Write(item.IsEnabled);
                    Write(item.IsReadonly);
                    Write(item.InvalidValidations?.Length > 0 ? string.Join("-", item.InvalidValidations) : null);
                    Write(item.WarningValidations?.Length > 0 ? string.Join("-", item.WarningValidations) : null);
                    Write(item.AsString);
                    Write(item.AsInt);
                    Write(item.AsLong);
                    Write(item.AsDouble);
                    Write(item.AsDateTime);
                    WriteJson(item.AsList);
                    Write(item.AsIntArray);
                    WriteJson(item.AsIntMatrix);
                    WriteJson(item.AsGps);
                    Write(item.AsBool);
                    WriteJson(item.AsYesNo);
                    WriteJson(item.AsAudio);
                    WriteJson(item.AsArea);
                    Write(item.HasFlag);

                    void Write<T>(T value)
                    {
                        if (value == null) importer.WriteNull();
                        else importer.Write(value);
                    }

                    void WriteJson<T>(T value)
                    {
                        if (value == null) importer.WriteNull();
                        else importer.Write(JsonConvert.SerializeObject(value), NpgsqlDbType.Jsonb);
                    }
                }
            }
        }

        public IEnumerable<InterviewEntity> GetInterviewEntities(IEnumerable<Guid> interviews)
        {
            var connection = sessionProvider.GetSession().Connection;

            var ids = string.Join(",", interviews.Select(i => "'" + i.ToString() + "'"));

            // for some reason Postgres decide that it's good to sequence scan whole interviews table
            // following line will ensure that Postgres will not do that
            connection.Execute("set enable_seqscan=false");

            var queryResult = connection.Query<InterviewEntityDto>(
                "SELECT interviewid, entityid, rostervector, isenabled, isreadonly, invalidvalidations, warnings, asstring, asint," +
                " aslong, asdouble, asdatetime, aslist, asintarray, asintmatrix, asgps, asbool, asyesno, asaudio, asarea, hasflag, entity_type as EntityType " +
                $" from {Table.InterviewsView} where {Column.InterviewId} in ({ids})", commandTimeout: 0, buffered: false);

            int[] ParseIntArray(string array, char delimeter = '-')
            {
                if (string.IsNullOrWhiteSpace(array) || string.IsNullOrWhiteSpace(array.Trim('_'))) return null;

                var items = array.Split(delimeter);

                var result = new int[items.Length];

                for (int i = 0; i < items.Length; i++)
                {
                    result[i] = int.Parse(items[i]);
                }

                return result;
            }

            foreach (var result in queryResult)
            {
                var entity = new InterviewEntity();

                entity.InterviewId = result.InterviewId;
                entity.Identity = new Identity(result.EntityId, ParseIntArray(result.RosterVector) ?? RosterVector.Empty);

                entity.IsEnabled = result.IsEnabled;
                entity.IsReadonly = result.IsReadonly;
                entity.InvalidValidations = ParseIntArray(result.InvalidValidations);
                entity.WarningValidations = ParseIntArray(result.Warnings);
                entity.AsString = result.AsString;
                entity.AsInt = result.AsInt;
                entity.AsLong = result.AsLong;
                entity.AsDouble = result.AsDouble;
                entity.AsDateTime = result.AsDateTime;
                entity.AsIntArray = result.AsIntArray;
                entity.AsBool = result.AsBool;
                entity.HasFlag = result.HasFlag;

                entity.AsList = Deserialize<InterviewTextListAnswer[]>(result.AsList);
                entity.AsIntMatrix = Deserialize<int[][]>(result.AsIntMatrix);
                entity.AsGps = Deserialize<GeoPosition>(result.AsGps);
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

        #region Obsolete InterviewData
        public List<InterviewEntity> GetInterviewEntities(Guid interviews)
        {
            return GetInterviewEntities(new[] { interviews }).ToList();
        }

        public Dictionary<string, InterviewLevel> GetInterviewDataLevels(IQuestionnaire questionnaire, List<InterviewEntity> interviewEntities)
        {
            var interviewLevels = interviewEntities
                .GroupBy(x => x.Identity?.RosterVector ?? RosterVector.Empty)
                .Select(x => ToInterviewLevel(x.Key, x.ToArray(), questionnaire));

            var interviewDataLevels = interviewLevels.ToDictionary(k => CreateLevelIdFromPropagationVector(k.RosterVector), v => v);
            return interviewDataLevels;
        }

        private InterviewLevel ToInterviewLevel(RosterVector rosterVector, InterviewEntity[] interviewDbEntities, IQuestionnaire questionnaire)
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
                        var question = ToQuestion(entity);
                        interviewLevel.QuestionsSearchCache.Add(question.Id, question);
                        break;
                    case EntityType.Variable:
                        interviewLevel.Variables.Add(entity.Identity.Id, ToObjectAnswer(entity));
                        if (entity.IsEnabled == false)
                            interviewLevel.DisabledVariables.Add(entity.Identity.Id);
                        break;
                }
            }

            return interviewLevel;
        }

        private InterviewQuestion ToQuestion(InterviewEntity entity)
        {
            var objectAnswer = ToObjectAnswer(entity);

            return new InterviewQuestion
            {
                Id = entity.Identity.Id,
                Answer = objectAnswer,
                FailedValidationConditions = entity.InvalidValidations?.Select(x => new FailedValidationCondition(x)).ToReadOnlyCollection(),
                FailedWarningConditions = entity.WarningValidations?.Select(x => new FailedValidationCondition(x)).ToReadOnlyCollection(),
                QuestionState = ToQuestionState(entity, objectAnswer != null)
            };
        }

        private QuestionState ToQuestionState(InterviewEntity entity, bool hasAnswer)
        {
            QuestionState state = 0;

            if (entity.IsEnabled) state = state.With(QuestionState.Enabled);
            if (entity.IsReadonly) state = state.With(QuestionState.Readonly);
            if (entity.InvalidValidations == null) state = state.With(QuestionState.Valid);
            if (entity.HasFlag) state = state.With(QuestionState.Flagged);
            if (hasAnswer) state = state.With(QuestionState.Answered);

            return state;
        }

        private object ToObjectAnswer(InterviewEntity entity) => entity.AsString ?? entity.AsInt ?? entity.AsDouble ??
                                                                 entity.AsDateTime ?? entity.AsLong ??
                                                                 entity.AsBool ?? entity.AsGps ?? entity.AsIntArray ??
                                                                 entity.AsList ?? entity.AsYesNo ??
                                                                 entity.AsIntMatrix ?? entity.AsArea ??
                                                                 (object)entity.AsAudio;

        public static string CreateLevelIdFromPropagationVector(decimal[] vector)
        {
            if (vector.Length == 0)
                return "#";
            return vector.CreateLeveKeyFromPropagationVector();
        }
        #endregion

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
            public const string HasFlag = "hasflag";
            public const string IsEnabled = "isenabled";
            public const string AsGpsColumn = "asgps";
            public const string AsString = "asstring";
            public const string AsAudio = "asaudio";
            public const string QuestionnaireIdentity = "questionnaireidentity";
        }

        private static class Table
        {
            public const string Interviews = "readside.interviews";
            public const string InterviewSummaries = "readside.interviewsummaries";
            public const string QuestionnaireEntities = "readside.questionnaire_entities";
            public const string InterviewsView = "readside.interviews_view";
        }
    }
}
