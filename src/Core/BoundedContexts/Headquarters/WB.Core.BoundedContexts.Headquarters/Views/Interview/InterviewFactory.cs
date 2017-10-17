using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Main.Core.Entities.SubEntities;
using Npgsql;
using NpgsqlTypes;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewFactory : IInterviewFactory
    {
        private const string PrimaryKeyConstraintName = "pk_interviews";
        private const string InterviewsTableName = "readside.interviews";
        private const string InterviewIdColumn = "interviewId";
        private const string EntityIdColumn = "entityId";
        private const string RosterVectorColumn = "rostervector";
        private const string EntityTypeColumn = "entitytype";
        private const string AnswerTypeColumn = "answertype";
        private const string ReadOnlyColumn = "isreadonly";
        private const string EnabledColumn = "isenabled";
        private const string InvalidValidationsColumn = "invalidvalidations";
        private const string FlagColumn = "hasflag";

        private const string AsIntColumn = "asint";
        private const string AsDoubleColumn = "asdouble";
        private const string AsLongColumn = "aslong";
        private const string AsDateTimeColumn = "asdatetime";
        private const string AsStringColumn = "asstring";
        private const string AsListColumn = "aslist";
        private const string AsIntArrayColumn = "asintarray";
        private const string AsIntMatrixColumn = "asintmatrix";
        private const string AsYesNoColumn = "asyesno";
        private const string AsGpsColumn = "asgps";
        private const string AsBoolColumn = "asbool";
        private const string AsAudioColumn = "asaudio";
        private const string AsAreaColumn = "asarea";
        
        private static readonly AnswerType[] JsonAnswerTypes =
        {
            AnswerType.Area, AnswerType.Audio, AnswerType.Gps, AnswerType.IntMatrix, AnswerType.TextList,
            AnswerType.YesNoList
        };

        private readonly Dictionary<AnswerType, string> AnswerColumnNameByAnswerType = new Dictionary<AnswerType, string>
        {
            {AnswerType.Area, AsAreaColumn},
            {AnswerType.Audio, AsAudioColumn},
            {AnswerType.Bool, AsBoolColumn},
            {AnswerType.Datetime, AsDateTimeColumn},
            {AnswerType.Double, AsDoubleColumn},
            {AnswerType.Gps, AsGpsColumn},
            {AnswerType.Int, AsIntColumn},
            {AnswerType.IntArray, AsIntArrayColumn},
            {AnswerType.IntMatrix, AsIntMatrixColumn},
            {AnswerType.Long, AsLongColumn},
            {AnswerType.String, AsStringColumn},
            {AnswerType.TextList, AsListColumn},
            {AnswerType.YesNoList, AsYesNoColumn}
        };

        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> summaryRepository;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly ISessionProvider sessionProvider;
        private readonly IEntitySerializer<object> jsonSerializer;

        public InterviewFactory(
            IQueryableReadSideRepositoryReader<InterviewSummary> summaryRepository,
            IQuestionnaireStorage questionnaireStorage,
            ISessionProvider sessionProvider,
            IEntitySerializer<object> jsonSerializer)
        {
            this.summaryRepository = summaryRepository;
            this.questionnaireStorage = questionnaireStorage;
            this.sessionProvider = sessionProvider;
            this.jsonSerializer = jsonSerializer;
        }

        public Identity[] GetFlaggedQuestionIds(Guid interviewId)
            => this.sessionProvider.GetSession().Connection.Query(
                    $"SELECT {EntityIdColumn}, {RosterVectorColumn} FROM {InterviewsTableName} WHERE {InterviewIdColumn} = @InterviewId AND {FlagColumn} = true",
                    new {InterviewId = interviewId})
                .Select(x => Identity.Create((Guid) x.entityid, (int[]) x.rostervector))
                .ToArray();

        public void SetFlagToQuestion(Guid interviewId, Identity questionIdentity, bool flagged)
        {
            this.ThrowIfInterviewDeletedOrReadOnly(interviewId);

            this.sessionProvider.GetSession().Connection.Execute(
                $"INSERT INTO {InterviewsTableName} ({InterviewIdColumn}, {EntityIdColumn}, {RosterVectorColumn}, {EntityTypeColumn}, {FlagColumn}) " +
                $"VALUES(@InterviewId, @EntityId, @RosterVector, @EntityType, {flagged}) " +
                $"ON CONFLICT ON CONSTRAINT {PrimaryKeyConstraintName} " +
                "DO UPDATE SET " +
                $"{FlagColumn} = {flagged};",
                new
                {
                    InterviewId = interviewId,
                    EntityId = questionIdentity.Id,
                    RosterVector = questionIdentity.RosterVector.ToArray(),
                    EntityType = EntityType.Question
                });
        }

        public void RemoveInterview(Guid interviewId)
            => this.sessionProvider.GetSession().Connection.Execute(
                $"DELETE FROM {InterviewsTableName} WHERE {InterviewIdColumn} = @InterviewId",
                new[] {new {InterviewId = interviewId}});

        public void UpdateAnswer(Guid interviewId, Identity questionIdentity, object answer)
            => UpdateAnswer(interviewId, questionIdentity, answer, EntityType.Question);

        public void UpdateVariables(Guid interviewId, ChangedVariable[] variables)
            => variables.ForEach(variable => this.UpdateAnswer(interviewId, variable.Identity, variable.NewValue,
                EntityType.Variable));

        private void UpdateAnswer(Guid interviewId, Identity questionIdentity, object answer, EntityType entityType)
        {
            var answerType = InterviewEntity.GetAnswerType(answer);
            var columnNameByAnswer = AnswerColumnNameByAnswerType[answerType.Value];
            var isJsonAnswer = JsonAnswerTypes.Contains(answerType.Value);
            
            using (var command = this.sessionProvider.GetSession().Connection.CreateCommand())
            {
                command.CommandText =
                    $"INSERT INTO {InterviewsTableName} ({InterviewIdColumn}, {EntityIdColumn}, {RosterVectorColumn}, {EntityTypeColumn}, {AnswerTypeColumn}, {columnNameByAnswer}) " +
                    $"VALUES(@{InterviewIdColumn}, @{EntityIdColumn}, @{RosterVectorColumn}, @{EntityTypeColumn}, @{AnswerTypeColumn}, @Answer) " +
                    $"ON CONFLICT ON CONSTRAINT {PrimaryKeyConstraintName} " +
                    "DO UPDATE SET " +
                    $"{columnNameByAnswer} = @Answer;";

                command.Parameters.Add(new NpgsqlParameter(InterviewIdColumn, NpgsqlDbType.Uuid){ Value = interviewId});
                command.Parameters.Add(new NpgsqlParameter(EntityIdColumn, NpgsqlDbType.Uuid) { Value = questionIdentity.Id });
                command.Parameters.Add(new NpgsqlParameter(RosterVectorColumn, NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = questionIdentity.RosterVector.ToArray() });
                command.Parameters.Add(new NpgsqlParameter(EntityTypeColumn, NpgsqlDbType.Integer) { Value = entityType });
                command.Parameters.Add(new NpgsqlParameter(AnswerTypeColumn, NpgsqlDbType.Integer) { Value = answerType });
                command.Parameters.Add(isJsonAnswer
                    ? new NpgsqlParameter("Answer", NpgsqlDbType.Jsonb){Value = this.jsonSerializer.Serialize(answer)}
                    : new NpgsqlParameter("Answer", answer));

                command.ExecuteNonQuery();
            }
        }

        public void MakeEntitiesValid(Guid interviewId, Identity[] entityIds, EntityType entityType)
            => this.sessionProvider.GetSession().Connection.Execute(
                $"INSERT INTO {InterviewsTableName} ({InterviewIdColumn}, {EntityIdColumn}, {RosterVectorColumn}, {EntityTypeColumn}) " +
                "VALUES(@InterviewId, @EntityId, @RosterVector, @EntityType) " +
                $"ON CONFLICT ON CONSTRAINT {PrimaryKeyConstraintName} " +
                "DO UPDATE SET " +
                $"{InvalidValidationsColumn} = null;",
                entityIds.Select(x => new
                {
                    InterviewId = interviewId,
                    EntityId = x.Id,
                    RosterVector = x.RosterVector.ToArray(),
                    EntityType = entityType
                }));

        public void MakeEntitiesInvalid(Guid interviewId,
            IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>> entityIds, EntityType entityType)
            => this.sessionProvider.GetSession().Connection.Execute(
                $"INSERT INTO {InterviewsTableName} ({InterviewIdColumn}, {EntityIdColumn}, {RosterVectorColumn}, {EntityTypeColumn}, {InvalidValidationsColumn}) " +
                "VALUES(@InterviewId, @EntityId, @RosterVector, @EntityType, @InvalidValidations) " +
                $"ON CONFLICT ON CONSTRAINT {PrimaryKeyConstraintName} " +
                "DO UPDATE SET " +
                $"{InvalidValidationsColumn} = @InvalidValidations;",
                entityIds.Select(x => new
                {
                    InterviewId = interviewId,
                    EntityId = x.Key.Id,
                    RosterVector = x.Key.RosterVector.ToArray(),
                    EntityType = entityType,
                    InvalidValidations = x.Value.Select(y => y.FailedConditionIndex).ToArray()
                }));

        public void EnableEntities(Guid interviewId, Identity[] entityIds, EntityType entityType, bool isEnabled)
            => this.sessionProvider.GetSession().Connection.Execute(
                $"INSERT INTO {InterviewsTableName} ({InterviewIdColumn}, {EntityIdColumn}, {RosterVectorColumn}, {EntityTypeColumn}, {EnabledColumn}) " +
                "VALUES(@InterviewId, @EntityId, @RosterVector, @EntityType, @Enabled) " +
                $"ON CONFLICT ON CONSTRAINT {PrimaryKeyConstraintName} " +
                "DO UPDATE SET " +
                $"{EnabledColumn} = {isEnabled};",
                entityIds.Select(x => new
                {
                    InterviewId = interviewId,
                    EntityId = x.Id,
                    RosterVector = x.RosterVector.ToArray(),
                    EntityType = entityType,
                    Enabled = isEnabled
                }));

        public void MarkQuestionsAsReadOnly(Guid interviewId, Identity[] questionIds)
            => this.sessionProvider.GetSession().Connection.Execute(
                $"INSERT INTO {InterviewsTableName} ({InterviewIdColumn}, {EntityIdColumn}, {RosterVectorColumn}, {EntityTypeColumn}, {ReadOnlyColumn}) " +
                "VALUES(@InterviewId, @EntityId, @RosterVector, @EntityType, true) " +
                $"ON CONFLICT ON CONSTRAINT {PrimaryKeyConstraintName} " +
                "DO UPDATE SET " +
                $"{ReadOnlyColumn} = true;",
                questionIds.Select(x => new
                {
                    InterviewId = interviewId,
                    EntityId = x.Id,
                    RosterVector = x.RosterVector.ToArray(),
                    EntityType = EntityType.Question
                }));

        public void AddRosters(Guid interviewId, Identity[] rosterIds)
            => this.sessionProvider.GetSession().Connection.Execute(
                $"INSERT INTO {InterviewsTableName} ({InterviewIdColumn}, {EntityIdColumn}, {RosterVectorColumn}, {EntityTypeColumn}) " +
                $"VALUES(@InterviewId, @EntityId, @RosterVector, @EntityType);",
                rosterIds.Select(x => new
                {
                    InterviewId = interviewId,
                    EntityId = x.Id,
                    RosterVector = x.RosterVector.ToArray(),
                    EntityType = EntityType.Section
                }));

        public void RemoveRosters(QuestionnaireIdentity questionnaireId, Guid interviewId, Identity[] rosterIds)
        {
            var questionnaire = this.questionnaireStorage.GetQuestionnaireDocument(questionnaireId);
            if (questionnaire == null) return;

            var removedEntityIds = new List<Identity>();
            foreach (var instance in rosterIds)
            {
                var roster = questionnaire.Find<IGroup>(instance.Id);

                var questionsIds = roster.Children
                    .TreeToEnumerableDepthFirst(x => x.Children)
                    .OfType<IQuestion>().Select(x => x.PublicKey).ToArray();

                var groupIds = roster.Children
                    .TreeToEnumerableDepthFirst(x => x.Children)
                    .OfType<IGroup>().Select(x => x.PublicKey).Concat(new[] {instance.Id}).ToArray();

                var variablesIds = roster.Children
                    .TreeToEnumerableDepthFirst(x => x.Children)
                    .OfType<IVariable>().Select(x => x.PublicKey).ToArray();

                var staticTextsIds = roster.Children
                    .TreeToEnumerableDepthFirst(x => x.Children)
                    .OfType<IStaticText>().Select(x => x.PublicKey).ToArray();

                removedEntityIds.AddRange(questionsIds.Union(groupIds).Union(variablesIds).Union(staticTextsIds)
                    .Select(x => Identity.Create(x, instance.RosterVector)));
            }

            var sqlParams = removedEntityIds.Select(x => new
            {
                InterviewId = interviewId,
                EntityId = x.Id,
                RosterVector = x.RosterVector.ToArray(),
            });

            this.sessionProvider.GetSession().Connection.Execute($"DELETE FROM {InterviewsTableName} " +
                                                                   $"WHERE {InterviewIdColumn} = @InterviewId " +
                                                                   $"AND {EntityIdColumn} = @EntityId " +
                                                                   $"AND {RosterVectorColumn} = @RosterVector;",
                sqlParams);

        }

        public void RemoveAnswers(Guid interviewId, Identity[] questionIds)
            => this.sessionProvider.GetSession().Connection.Execute(
                $"INSERT INTO {InterviewsTableName} ({InterviewIdColumn}, {EntityIdColumn}, {RosterVectorColumn}, {EntityTypeColumn}) " +
                "VALUES(@InterviewId, @EntityId, @RosterVector, @EntityType) " +
                $"ON CONFLICT ON CONSTRAINT {PrimaryKeyConstraintName} " +
                "DO UPDATE SET " +
                $"{AsAreaColumn} = null, " +
                $"{AsAudioColumn} = null, " +
                $"{AsBoolColumn} = null, " +
                $"{AsDateTimeColumn} = null, " +
                $"{AsDoubleColumn} = null, " +
                $"{AsGpsColumn} = null, " +
                $"{AsIntArrayColumn} = null, " +
                $"{AsIntColumn} = null, " +
                $"{AsIntMatrixColumn} = null, " +
                $"{AsListColumn} = null, " +
                $"{AsLongColumn} = null, " +
                $"{AsStringColumn} = null, " +
                $"{AsYesNoColumn} = null;",
                questionIds.Select(x => new
                {
                    InterviewId = interviewId,
                    EntityId = x.Id,
                    RosterVector = x.RosterVector.ToArray(),
                    EntityType = EntityType.Question
                }));

        public InterviewStringAnswer[] GetAllMultimediaAnswers(Guid[] multimediaQuestionIds)
        {
            if (!multimediaQuestionIds?.Any() ?? true) return EmptyArray<InterviewStringAnswer>.Value;

            return this.sessionProvider.GetSession().Connection
                .Query<InterviewStringAnswer>($"SELECT {InterviewIdColumn}, {AsStringColumn} as answer " +
                                              $"FROM {InterviewsTableName} " +
                                              $"WHERE {EnabledColumn} = true " +
                                              $"AND {AsStringColumn} IS NOT NULL " +
                                              $"AND {EntityIdColumn} IN ({string.Join(",", multimediaQuestionIds.Select(x => $"'{x}'"))})")
                .ToArray();
        }

        public InterviewStringAnswer[] GetAllAudioAnswers()
            => this.sessionProvider.GetSession().Connection.Query<InterviewStringAnswer>(
                $"SELECT {InterviewIdColumn}, {AsAudioColumn}::json->'FileName' as Answer " +
                $"FROM {InterviewsTableName} " +
                $"WHERE {AsAudioColumn} IS NOT NULL " +
                $"AND {EnabledColumn} = true").ToArray();

        public Guid[] GetAnsweredGpsQuestionIdsByQuestionnaire(QuestionnaireIdentity questionnaireIdentity)
            => this.sessionProvider.GetSession().Connection.Query<Guid>(
                $"SELECT {EntityIdColumn} " +
                $"FROM readside.interviewsummaries s INNER JOIN {InterviewsTableName} i ON(s.interviewid = i.{InterviewIdColumn}) " +
                $"WHERE questionnaireidentity = '{questionnaireIdentity}' " +
                $"AND {AsGpsColumn} is not null " +
                $"GROUP BY {EntityIdColumn} ").ToArray();

        public string[] GetQuestionnairesWithAnsweredGpsQuestions()
            => this.sessionProvider.GetSession().Connection.Query<string>(
                $"SELECT questionnaireidentity " +
                $"FROM readside.interviewsummaries s INNER JOIN {InterviewsTableName} i ON(s.interviewid = i.{InterviewIdColumn}) " +
                $"WHERE {AsGpsColumn} is not null " +
                $"GROUP BY questionnaireidentity").ToArray();

        public InterviewGpsAnswer[] GetGpsAnswersByQuestionIdAndQuestionnaire(QuestionnaireIdentity questionnaireIdentity,
            Guid gpsQuestionId, int maxAnswersCount, double northEastCornerLatitude, double southWestCornerLatitude,
            double northEastCornerLongtitude, double southWestCornerLongtitude)
            => this.sessionProvider.GetSession().Connection.Query<InterviewGpsAnswer>(
                    $"SELECT i.{InterviewIdColumn}, {AsGpsColumn}->>'{nameof(GeoPosition.Latitude)}' as latitude, {AsGpsColumn}->>'{nameof(GeoPosition.Longitude)}' as longitude " +
                    $"FROM readside.interviewsummaries s INNER JOIN {InterviewsTableName} i ON(s.interviewid = i.{InterviewIdColumn}) " +
                    $"WHERE questionnaireidentity = @Questionnaire " +
                    $"AND {EntityIdColumn} = @QuestionId " +
                    $"AND {AsGpsColumn} is not null " +
                    $"AND ({AsGpsColumn} ->> '{nameof(GeoPosition.Latitude)}')::double precision > @SouthWestCornerLatitude " +
                    $"AND ({AsGpsColumn} ->> '{nameof(GeoPosition.Latitude)}')::double precision < @NorthEastCornerLatitude " +
                    $"AND ({AsGpsColumn} ->> '{nameof(GeoPosition.Longitude)}')::double precision > @SouthWestCornerLongtitude " +
                    $"{(northEastCornerLongtitude >= southWestCornerLongtitude ? "AND" : "OR")}" +
                    $" ({AsGpsColumn} ->> '{nameof(GeoPosition.Longitude)}')::double precision < @NorthEastCornerLongtitude " +
                    $"LIMIT @MaxCount",
                    new
                    {
                        Questionnaire = questionnaireIdentity.ToString(),
                        QuestionId = gpsQuestionId,
                        MaxCount = maxAnswersCount,
                        SouthWestCornerLatitude = southWestCornerLatitude,
                        NorthEastCornerLatitude = northEastCornerLatitude,
                        SouthWestCornerLongtitude = southWestCornerLongtitude,
                        NorthEastCornerLongtitude = northEastCornerLongtitude
                    })
                .ToArray();

        #region Obsolete InterviewData

        private IEntitySerializer<T> GetSerializer<T>() where T : class => ServiceLocator.Current.GetInstance<IEntitySerializer<T>>() ;

        public InterviewData GetInterviewData(Guid interviewId)
        {
            var interviewSummary = this.summaryRepository.GetById(interviewId.FormatGuid());
            var interviewData = new InterviewData
            {
                InterviewId = interviewId,
                ResponsibleId = interviewSummary.ResponsibleId,
                Status = interviewSummary.Status,
                AssignmentId = interviewSummary.AssignmentId,
                CreatedOnClient = interviewSummary.WasCreatedOnClient,
                HasErrors = interviewSummary.HasErrors,
                InterviewKey = interviewSummary.ClientKey,
                ReceivedByInterviewer = interviewSummary.ReceivedByInterviewer,
                ResponsibleRole = interviewSummary.ResponsibleRole,
                SupervisorId = interviewSummary.TeamLeadId,
                UpdateDate = interviewSummary.UpdateDate,
                WasCompleted = interviewSummary.WasCompleted,
                IsMissingAssignToInterviewer = !interviewSummary.IsAssignedToInterviewer
            };

            IQuestionnaire questionnaire =
                this.questionnaireStorage.GetQuestionnaire(
                    QuestionnaireIdentity.Parse(interviewSummary.QuestionnaireIdentity), null);

            var interviewEntites = this.sessionProvider.GetSession().Connection
                .Query($"SELECT * FROM {InterviewsTableName} WHERE {InterviewIdColumn} = @InterviewId", new {InterviewId = interviewId})
                .ToList()
                .Select(x => new InterviewEntity
                {
                    InterviewId = x.interviewid,
                    Identity = Identity.Create(x.entityid, x.rostervector),
                    EntityType = (EntityType)x.entitytype,
                    AnswerType = (AnswerType?)x.answertype,
                    IsEnabled = x.isenabled,
                    IsReadonly = x.isreadonly,
                    HasFlag = x.hasflag,
                    InvalidValidations = x.invalidvalidations,
                    AsString = x.asstring,
                    AsInt = x.asint,
                    AsBool = x.asbool,
                    AsDouble = x.asdouble,
                    AsDateTime = x.asdatetime,
                    AsLong = x.aslong,
                    AsIntArray = x.asintarray,
                    AsIntMatrix = x.asintmatrix == null ? null : this.GetSerializer<int[][]>().Deserialize(x.asintmatrix),
                    AsGps = x.asgps == null ? null : this.GetSerializer<GeoPosition>().Deserialize(x.asgps),
                    AsList = x.aslist == null ? null : this.GetSerializer<InterviewTextListAnswer[]>().Deserialize(x.aslist),
                    AsYesNo = x.asyesno == null ? null : this.GetSerializer<AnsweredYesNoOption[]>().Deserialize(x.asyesno),
                    AsAudio = x.asaudio == null ? null : this.GetSerializer<AudioAnswer>().Deserialize(x.asaudio),
                    AsArea = x.asarea == null ? null : this.GetSerializer<Area>().Deserialize(x.asarea),
                })
                .ToList();

            var groupBy = interviewEntites
                .GroupBy(x => x.Identity?.RosterVector ?? RosterVector.Empty)
                .ToDictionary(x => x.Key, x => x.ToArray());

            var interviewLevels = groupBy.Select(x => ToInterviewLevel(x.Key, x.Value, questionnaire)).ToList();

            interviewData.Levels =
                interviewLevels.ToDictionary(k => CreateLevelIdFromPropagationVector(k.RosterVector), v => v);

            return interviewData;
        }
        private InterviewLevel ToInterviewLevel(RosterVector rosterVector, InterviewEntity[] interviewDbEntities,
            IQuestionnaire questionnaire)
        {
            Dictionary<ValueVector<Guid>, int?> scopeVectors = new Dictionary<ValueVector<Guid>, int?>();
            if (rosterVector.Length > 0)
            {
                // too slow
                scopeVectors = interviewDbEntities
                    .Select(x => questionnaire.GetRosterSizeSourcesForEntity(x.Identity.Id))
                    .Select(x => new ValueVector<Guid>(x))
                    .Distinct()
                    .ToDictionary(x => x, x => (int?) 0);
            }
            else
            {
                scopeVectors.Add(new ValueVector<Guid>(), 0);
            }

            var disabledGroups = interviewDbEntities
                .Where(x => x.EntityType == EntityType.Section && x.IsEnabled == false).Select(x => x.Identity.Id)
                .ToHashSet();

            var disabledVariables = interviewDbEntities
                .Where(x => x.EntityType == EntityType.Variable && x.IsEnabled == false).Select(x => x.Identity.Id)
                .ToHashSet();

            var dictionary = interviewDbEntities.Where(x => x.EntityType == EntityType.Variable)
                .Select(x => new {x.Identity.Id, Answer = ToObjectAnswer(x)})
                .ToDictionary(x => x.Id, x => x.Answer);

            var interviewStaticTexts = interviewDbEntities.Where(x => x.EntityType == EntityType.StaticText)
                .Select(ToStaticText).ToDictionary(x => x.Id);

            var questionsSearchCache = interviewDbEntities.Where(x => x.EntityType == EntityType.Question)
                .Select(ToQuestion).ToDictionary(x => x.Id);

            return new InterviewLevel
            {
                RosterVector = rosterVector,
                DisabledGroups = disabledGroups,
                DisabledVariables = disabledVariables,
                Variables = dictionary,
                StaticTexts = interviewStaticTexts,
                QuestionsSearchCache = questionsSearchCache,
                ScopeVectors = scopeVectors
            };
        }

        private InterviewQuestion ToQuestion(InterviewEntity entity)
        {
            var objectAnswer = ToObjectAnswer(entity);

            return new InterviewQuestion
            {
                Id = entity.Identity.Id,
                Answer = objectAnswer,
                FailedValidationConditions = entity.InvalidValidations?.Select(x => new FailedValidationCondition(x)).ToReadOnlyCollection(),
                QuestionState = ToQuestionState(entity, objectAnswer != null)
            };
        }

        private QuestionState ToQuestionState(InterviewEntity entity, bool hasAnswer)
        {
            QuestionState state = 0;

            if(entity.IsEnabled)
                state = state.With(QuestionState.Enabled);

            if (entity.IsReadonly)
                state = state.With(QuestionState.Readonly);

            if (entity.InvalidValidations == null)
                state = state.With(QuestionState.Valid);

            if (entity.HasFlag)
                state = state.With(QuestionState.Flagged);

            if (hasAnswer)
                state = state.With(QuestionState.Answered);

            return state;
        }

        private InterviewStaticText ToStaticText(InterviewEntity entity) => new InterviewStaticText
        {
            Id = entity.Identity.Id,
            IsEnabled = entity.IsEnabled,
            FailedValidationConditions = (entity.InvalidValidations?.Select(x => new FailedValidationCondition(x)) ??
                                          new FailedValidationCondition[0]).ToReadOnlyCollection()
        };

        private object ToObjectAnswer(InterviewEntity entity) => entity.AsString ?? entity.AsDouble ?? entity.AsInt ??
                                                                 entity.AsDateTime ?? entity.AsLong ??
                                                                 entity.AsBool ?? entity.AsGps ?? entity.AsIntArray ??
                                                                 entity.AsList ?? entity.AsYesNo ??
                                                                 entity.AsIntMatrix ?? entity.AsArea ??
                                                                 (object) entity.AsAudio;

        public static string CreateLevelIdFromPropagationVector(decimal[] vector)
        {
            if (vector.Length == 0)
                return "#";
            return vector.CreateLeveKeyFromPropagationVector();
        }
        #endregion

        private void ThrowIfInterviewDeletedOrReadOnly(Guid interviewId)
        {
            var interview = this.summaryRepository.GetById(interviewId);

            if (interview == null)
                throw new InterviewException($"Interview {interviewId} not found.");

            ThrowIfInterviewApprovedByHq(interview);
            ThrowIfInterviewReceivedByInterviewer(interview);
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
    }
}