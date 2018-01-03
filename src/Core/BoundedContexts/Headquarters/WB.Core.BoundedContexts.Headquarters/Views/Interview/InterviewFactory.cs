using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters;
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
using WB.Infrastructure.Native.Storage;
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

        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> summaryRepository;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly ISessionProvider sessionProvider;

        public InterviewFactory(
            IQueryableReadSideRepositoryReader<InterviewSummary> summaryRepository,
            IQuestionnaireStorage questionnaireStorage,
            ISessionProvider sessionProvider)
        {
            this.summaryRepository = summaryRepository;
            this.questionnaireStorage = questionnaireStorage;
            this.sessionProvider = sessionProvider;

            SqlMapper.AddTypeHandler(typeof(int[][]), new JsonHandler<int[][]>());
            SqlMapper.AddTypeHandler(typeof(GeoPosition), new JsonHandler<GeoPosition>());
            SqlMapper.AddTypeHandler(typeof(InterviewTextListAnswer[]), new JsonHandler<InterviewTextListAnswer[]>());
            SqlMapper.AddTypeHandler(typeof(AnsweredYesNoOption[]), new JsonHandler<AnsweredYesNoOption[]>());
            SqlMapper.AddTypeHandler(typeof(AudioAnswer), new JsonHandler<AudioAnswer>());
            SqlMapper.AddTypeHandler(typeof(Area), new JsonHandler<Area>());
        }

        public Identity[] GetQuestionsWithFlagBySectionId(QuestionnaireIdentity questionnaireId, Guid interviewId, Identity sectionId)
        {
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(questionnaireId, null);
            var questionsInSection = questionnaire.GetChildQuestions(sectionId.Id);

            if (!questionsInSection.Any()) return Array.Empty<Identity>();

            return this.sessionProvider.GetSession().Connection.Query(
                    $"SELECT {EntityIdColumn}, {RosterVectorColumn} " +
                    $"FROM {InterviewsTableName} " +
                    $"WHERE {InterviewIdColumn} = @InterviewId " +
                    $"AND {FlagColumn} = true " +
                    $"AND {EntityIdColumn} IN ({string.Join(",", questionsInSection.Select(x => $"'{x}'"))})" +
                    $"AND {RosterVectorColumn} = @RosterVector",
                    new {InterviewId = interviewId, RosterVector = sectionId.RosterVector.Array})
                .Select(x => Identity.Create((Guid) x.entityid, (int[]) x.rostervector))
                .ToArray();
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
                    RosterVector = questionIdentity.RosterVector.Array,
                    EntityType = EntityType.Question
                });
        }

        public void RemoveInterview(Guid interviewId)
            => this.sessionProvider.GetSession().Connection.Execute(
                $"DELETE FROM {InterviewsTableName} WHERE {InterviewIdColumn} = @InterviewId",
                new[] {new {InterviewId = interviewId}});

        public InterviewStringAnswer[] GetMultimediaAnswersByQuestionnaire(QuestionnaireIdentity questionnaireIdentity, Guid[] multimediaQuestionIds)
        {
            if (!multimediaQuestionIds?.Any() ?? true) return EmptyArray<InterviewStringAnswer>.Value;

            return this.sessionProvider.GetSession().Connection
                .Query<InterviewStringAnswer>($"SELECT i.{InterviewIdColumn}, i.{AsStringColumn} as answer " +
                                              $"FROM readside.interviewsummaries s INNER JOIN {InterviewsTableName} i ON(s.interviewid = i.{InterviewIdColumn}) " +
                                              $"WHERE questionnaireidentity = '{questionnaireIdentity}' " +
                                              $"AND {EnabledColumn} = true " +
                                              $"AND {AsStringColumn} IS NOT NULL " +
                                              $"AND {EntityIdColumn} IN ({string.Join(",", multimediaQuestionIds.Select(x => $"'{x}'"))})")
                .ToArray();
        }

        public InterviewStringAnswer[] GetAudioAnswersByQuestionnaire(QuestionnaireIdentity questionnaireIdentity)
            => this.sessionProvider.GetSession().Connection.Query<InterviewStringAnswer>(
                $"SELECT i.{InterviewIdColumn}, i.{AsAudioColumn}->>'{nameof(AudioAnswer.FileName)}' as Answer " +
                $"FROM readside.interviewsummaries s INNER JOIN {InterviewsTableName} i ON(s.interviewid = i.{InterviewIdColumn}) " +
                $"WHERE questionnaireidentity = '{questionnaireIdentity}' " +
                $"AND {AsAudioColumn} IS NOT NULL " +
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

        public List<ExportedError> GetErrors(IEnumerable<Guid> interveiws)
        {
            var connection = this.sessionProvider.GetSession().Connection;
            var array = interveiws.ToArray();
            var errors = connection.Query<ExportedError>(
                $@"SELECT {InterviewIdColumn} as {nameof(ExportedError.InterviewId)}, {EntityIdColumn} as {nameof(ExportedError.EntityId)}, 
                          {RosterVectorColumn} as {nameof(ExportedError.RosterVector)}, {EntityTypeColumn} as {nameof(ExportedError.EntityType)}, 
                          {InvalidValidationsColumn} as {nameof(ExportedError.FailedValidationConditions)}
                          FROM {InterviewsTableName}
                          WHERE {InterviewIdColumn} = ANY(@interviews) AND {EntityTypeColumn} IN(2, 3) AND array_length({InvalidValidationsColumn}, 1) > 0
                          ORDER BY interviewid",
                new
                {
                    interviews = array
                }).ToList();

            return errors;
        }

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

        public void Save(QuestionnaireIdentity questionnaireId, InterviewState state)
        {
            if (state.Removed.Any())
                this.RemoveEntities(state.Id, state.Removed.ToArray());

            if (state.ReadOnly.Any())
                this.MarkQuestionsAsReadOnly(state.Id, state.ReadOnly.ToArray());

            var questionnaire = this.questionnaireStorage.GetQuestionnaireDocument(questionnaireId);

            if (state.Enablement.Any())
                this.EnableOrDisableEntities(state.Id, state.Enablement, questionnaire);

            if (state.Validity.Any())
                this.MakeEntitiesValidOrInvalid(state.Id, state.Validity, questionnaire);

            if (state.Answers.Any())
                this.UpdateAnswers(state.Id, state.Answers, questionnaire);
        }

        private void RemoveEntities(Guid interviewId, Identity[] entities)
        {
            sessionProvider.GetSession().Connection.Execute(
                $"DELETE FROM {InterviewsTableName} " +
                $"WHERE {InterviewIdColumn} = @InterviewId " +
                $"AND {EntityIdColumn} = @EntityId " +
                $"AND {RosterVectorColumn} = @RosterVector",
                entities.Select(x => new
                {
                    InterviewId = interviewId,
                    EntityId = x.Id,
                    RosterVector = x.RosterVector.Array
                }).ToArray());
        }

        private void MakeEntitiesValidOrInvalid(Guid interviewId, Dictionary<Identity, int[]> validity, QuestionnaireDocument questionnaire)
            => this.sessionProvider.GetSession().Connection.Execute(
                $"INSERT INTO {InterviewsTableName} ({InterviewIdColumn}, {EntityIdColumn}, {RosterVectorColumn}, {EntityTypeColumn}, {InvalidValidationsColumn}) " +
                "VALUES(@InterviewId, @EntityId, @RosterVector, @EntityType, @InvalidValidations) " +
                $"ON CONFLICT ON CONSTRAINT {PrimaryKeyConstraintName} " +
                "DO UPDATE SET " +
                $"{InvalidValidationsColumn} = @InvalidValidations;",
                validity.Select(x => new
                {
                    InterviewId = interviewId,
                    EntityId = x.Key.Id,
                    RosterVector = x.Key.RosterVector.Array,
                    EntityType = GetEntityType(x.Key.Id, questionnaire),
                    InvalidValidations = x.Value
                }));

        private void EnableOrDisableEntities(Guid interviewId, Dictionary<Identity, bool> enablement, QuestionnaireDocument questionnaire)
            => this.sessionProvider.GetSession().Connection.Execute(
                $"INSERT INTO {InterviewsTableName} ({InterviewIdColumn}, {EntityIdColumn}, {RosterVectorColumn}, {EntityTypeColumn}, {EnabledColumn}) " +
                "VALUES(@InterviewId, @EntityId, @RosterVector, @EntityType, @Enabled) " +
                $"ON CONFLICT ON CONSTRAINT {PrimaryKeyConstraintName} " +
                "DO UPDATE SET " +
                $"{EnabledColumn} = @Enabled;",
                enablement.Select(x => new
                {
                    InterviewId = interviewId,
                    EntityId = x.Key.Id,
                    RosterVector = x.Key.RosterVector.Array,
                    EntityType = GetEntityType(x.Key.Id, questionnaire),
                    Enabled = x.Value
                }));

        private void MarkQuestionsAsReadOnly(Guid interviewId, Identity[] questionIds)
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
                    RosterVector = x.RosterVector.Array,
                    EntityType = EntityType.Question
                }));

        private void UpdateAnswers(Guid interviewId, Dictionary<Identity, object> answers, QuestionnaireDocument questionnaire)
            => this.sessionProvider.GetSession().Connection.Execute(
                $"INSERT INTO {InterviewsTableName} ({InterviewIdColumn}, {EntityIdColumn}, {RosterVectorColumn}, {EntityTypeColumn}, {AnswerTypeColumn}, {AsIntColumn}, {AsDoubleColumn}, " +
                $"{AsLongColumn}, {AsStringColumn}, {AsDateTimeColumn}, {AsBoolColumn}, {AsIntArrayColumn}, {AsListColumn}, {AsYesNoColumn}, {AsIntMatrixColumn}, {AsGpsColumn}, " +
                $"{AsAudioColumn}, {AsAreaColumn}) " +
                "VALUES(@InterviewId, @EntityId, @RosterVector, @EntityType, @AnswerType, @AsInt, @AsDouble, @AsLong, @AsString, @AsDateTime, @AsBool, @AsIntArray, " +
                "@AsList, @AsYesNo, @AsIntMatrix, @AsGps, @AsAudio, @AsArea) " +
                $"ON CONFLICT ON CONSTRAINT {PrimaryKeyConstraintName} " +
                "DO UPDATE SET " +
                $"{EntityTypeColumn} = @EntityType, " +
                $"{AnswerTypeColumn} = @AnswerType, " +
                $"{AsIntColumn} = @AsInt, " +
                $"{AsDoubleColumn} = @AsDouble, " +
                $"{AsLongColumn} = @AsLong, " +
                $"{AsStringColumn} = @AsString, " +
                $"{AsDateTimeColumn} = @AsDateTime, " +
                $"{AsBoolColumn} = @AsBool, " +
                $"{AsIntArrayColumn} = @AsIntArray, " +
                $"{AsListColumn} = @AsList, " +
                $"{AsYesNoColumn} = @AsYesNo, " +
                $"{AsIntMatrixColumn} = @AsIntMatrix, " +
                $"{AsGpsColumn} = @AsGps, " +
                $"{AsAudioColumn} = @AsAudio, " +
                $"{AsAreaColumn} = @AsArea;",
                answers.Select(x => new
                {
                    InterviewId = interviewId,
                    EntityId = x.Key.Id,
                    RosterVector = x.Key.RosterVector.Array,
                    EntityType = GetEntityType(x.Key.Id, questionnaire),
                    AnswerType = GetAnswerType(x.Key.Id, questionnaire),
                    AsInt = x.Value as int?,
                    AsDouble = x.Value as double?,
                    AsLong = x.Value as long?,
                    AsString = x.Value as string,
                    AsDateTime = x.Value as DateTime?,
                    AsBool = x.Value as bool?,
                    AsIntArray = x.Value as int[],
                    AsList = x.Value as InterviewTextListAnswer[],
                    AsYesNo = x.Value as AnsweredYesNoOption[],
                    AsIntMatrix = x.Value as int[][],
                    AsGps = x.Value as GeoPosition,
                    AsAudio = x.Value as AudioAnswer,
                    AsArea = x.Value as Area
                }));

        private AnswerType? GetAnswerType(Guid entityId, QuestionnaireDocument questionnaire)
        {
            var entity = questionnaire.Find<IComposite>(entityId);
            var variable = entity as IVariable;
            var question = entity as IQuestion;

            if (variable != null)
            {
                switch (variable.Type)
                {
                    case VariableType.Boolean:
                        return AnswerType.Bool;
                    case VariableType.DateTime:
                        return AnswerType.Datetime;
                    case VariableType.Double:
                        return AnswerType.Double;
                    case VariableType.LongInteger:
                        return AnswerType.Long;
                    case VariableType.String:
                        return AnswerType.String;
                }
            }

            if (question != null)
            {
                switch (question.QuestionType)
                {
                    case QuestionType.SingleOption:
                        if (question.LinkedToRosterId.HasValue ||
                            (question.LinkedToQuestionId.HasValue && questionnaire.Find<IQuestion>(question.LinkedToQuestionId.Value).QuestionType != QuestionType.TextList))
                            return AnswerType.IntArray;
                        else return AnswerType.Int;
                    case QuestionType.MultyOption:
                    {
                        if ((question as IMultyOptionsQuestion)?.YesNoView ?? false)
                            return AnswerType.YesNoList;

                        if (question.LinkedToRosterId.HasValue ||
                            (question.LinkedToQuestionId.HasValue && questionnaire.Find<IQuestion>(question.LinkedToQuestionId.Value).QuestionType != QuestionType.TextList))
                            return AnswerType.IntMatrix;

                        return AnswerType.IntArray;
                    }
                    case QuestionType.DateTime:
                        return AnswerType.Datetime;
                    case QuestionType.GpsCoordinates:
                        return AnswerType.Gps;
                    case QuestionType.Multimedia:
                        return AnswerType.String;
                    case QuestionType.Numeric:
                        if ((question as INumericQuestion)?.IsInteger ?? false)
                            return AnswerType.Int;
                        else return AnswerType.Double;
                    case QuestionType.QRBarcode:
                        return AnswerType.String;
                    case QuestionType.Area:
                        return AnswerType.Area;
                    case QuestionType.Text:
                        return AnswerType.String;
                    case QuestionType.TextList:
                        return AnswerType.TextList;
                    case QuestionType.Audio:
                        return AnswerType.Audio;
                }
            }

            return null;
        }

        private EntityType GetEntityType(Guid entityId, QuestionnaireDocument questionnaire)
        {
            var entity = questionnaire.Find<IComposite>(entityId);

            if (entity is IQuestion) return EntityType.Question;
            if (entity is IVariable) return EntityType.Variable;
            if (entity is IStaticText) return EntityType.StaticText;
            if (entity is IGroup) return EntityType.Section;

            throw new NotSupportedException("Unknown entity type");
        }

        #region Obsolete InterviewData

        private IEntitySerializer<T> GetSerializer<T>() where T : class =>
            ServiceLocator.Current.GetInstance<IEntitySerializer<T>>();
        
        public List<InterviewEntity> GetInterviewEntities(Guid interviewId)
        {
            var interviewEntites = sessionProvider.GetSession().Connection
                .Query<dynamic>($"SELECT * FROM {InterviewsTableName} WHERE {InterviewIdColumn} = @InterviewId",
                    new { InterviewId = interviewId })
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

            return interviewEntites;
        }

        public Dictionary<string, InterviewLevel> GetInterviewDataLevels(QuestionnaireIdentity questionnaireId, List<InterviewEntity> interviewEntities)
        {
            IQuestionnaire questionnaire =
                this.questionnaireStorage.GetQuestionnaire(questionnaireId, null);

            var interviewEntitiesGroupedByRosterVector = interviewEntities
                .GroupBy(x => x.Identity?.RosterVector ?? RosterVector.Empty)
                .ToDictionary(x => x.Key, x => x.ToArray());

            var interviewLevels = interviewEntitiesGroupedByRosterVector
                .Select(x => ToInterviewLevel(x.Key, x.Value, questionnaire)).ToList();

            var interviewDataLevels =
                interviewLevels.ToDictionary(k => CreateLevelIdFromPropagationVector(k.RosterVector), v => v);
            return interviewDataLevels;
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

        private object ToObjectAnswer(InterviewEntity entity) => entity.AsString ?? entity.AsInt ?? entity.AsDouble ?? 
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
            var interview = summaryRepository.GetById(interviewId);

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