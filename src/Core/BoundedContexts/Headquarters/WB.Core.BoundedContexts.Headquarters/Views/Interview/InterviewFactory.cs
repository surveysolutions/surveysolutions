using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Main.Core.Entities.SubEntities;
using Npgsql;
using NpgsqlTypes;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewFactory : IInterviewFactory
    {
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
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryRepository,
            IQuestionnaireStorage questionnaireStorage,
            ISessionProvider sessionProvider,
            IEntitySerializer<object> jsonSerializer)
        {
            this.summaryRepository = interviewSummaryRepository;
            this.questionnaireStorage = questionnaireStorage;
            this.sessionProvider = sessionProvider;
            this.jsonSerializer = jsonSerializer;
        }

        public Identity[] GetFlaggedQuestionIds(Guid interviewId)
            => this.sessionProvider.GetSession()?.Connection?.Query(
                    "SELECT entityid, rostervector FROM readside.interviews WHERE interviewid = @InterviewId AND hasflag = true",
                    new {InterviewId = interviewId})
                .Select(x => Identity.Create((Guid) x.entityid, (int[]) x.rostervector))
                .ToArray();

        public void SetFlagToQuestion(Guid interviewId, Identity questionIdentity, bool flagged)
        {
            this.ThrowIfInterviewDeletedOrReadOnly(interviewId);

            this.sessionProvider.GetSession()?.Connection?.Execute(
                $"INSERT INTO {InterviewsTableName} ({InterviewIdColumn}, {EntityIdColumn}, {RosterVectorColumn}, {EntityTypeColumn}, {FlagColumn}) " +
                $"VALUES(@InterviewId, @EntityId, @RosterVector, @EntityType, {flagged}) " +
                "ON CONFLICT ON CONSTRAINT uk_interview " +
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
            => this.sessionProvider.GetSession()?.Connection?.Execute(
                $"DELETE FROM {InterviewsTableName} WHERE {InterviewIdColumn} = @InterviewId",
                new[] {new {InterviewId = interviewId}});

        public void UpdateAnswer(Guid interviewId, Identity questionIdentity, object answer)
            => UpdateAnswer(interviewId, questionIdentity, answer, EntityType.Question);

        public void UpdateVariables(Guid interviewId, ChangedVariable[] variables)
            => variables.ForEach(variable => this.UpdateAnswer(interviewId, variable.Identity, variable.NewValue,
                EntityType.Variable));

        private void UpdateAnswer(Guid interviewId, Identity questionIdentity, object answer, EntityType entityType)
        {
            var answerType = InterviewDbEntity.GetAnswerType(answer);
            var columnNameByAnswer = AnswerColumnNameByAnswerType[answerType.Value];
            var isJsonAnswer = JsonAnswerTypes.Contains(answerType.Value);
            
            using (var command = this.sessionProvider.GetSession()?.Connection?.CreateCommand())
            {
                command.CommandText =
                    $"INSERT INTO {InterviewsTableName} ({InterviewIdColumn}, {EntityIdColumn}, {RosterVectorColumn}, {EntityTypeColumn}, {AnswerTypeColumn}, {columnNameByAnswer}) " +
                    $"VALUES(@{InterviewIdColumn}, @{EntityIdColumn}, @{RosterVectorColumn}, @{EntityTypeColumn}, @{AnswerTypeColumn}, @Answer) " +
                    "ON CONFLICT ON CONSTRAINT uk_interview " +
                    "DO UPDATE SET " +
                    $"{columnNameByAnswer} = @Answer;";

                command.Parameters.Add(new NpgsqlParameter(InterviewIdColumn, NpgsqlDbType.Uuid){ Value = interviewId});
                command.Parameters.Add(new NpgsqlParameter(EntityIdColumn, NpgsqlDbType.Uuid) { Value = questionIdentity.Id });
                command.Parameters.Add(new NpgsqlParameter(RosterVectorColumn, NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = questionIdentity.RosterVector.ToArray() });
                command.Parameters.Add(new NpgsqlParameter(EntityTypeColumn, NpgsqlDbType.Integer) { Value = entityType });
                command.Parameters.Add(new NpgsqlParameter(AnswerTypeColumn, NpgsqlDbType.Integer) { Value = answerType });
                command.Parameters.Add(isJsonAnswer
                    ? new NpgsqlParameter("Answer", NpgsqlDbType.Json){Value = this.jsonSerializer.Serialize(answer)}
                    : new NpgsqlParameter("Answer", answer));

                command.ExecuteNonQuery();
            }
        }

        public void MakeEntitiesValid(Guid interviewId, Identity[] entityIds, EntityType entityType)
            => this.sessionProvider.GetSession()?.Connection?.Execute(
                $"INSERT INTO {InterviewsTableName} ({InterviewIdColumn}, {EntityIdColumn}, {RosterVectorColumn}, {EntityTypeColumn}) " +
                "VALUES(@InterviewId, @EntityId, @RosterVector, @EntityType) " +
                "ON CONFLICT ON CONSTRAINT uk_interview " +
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
            => this.sessionProvider.GetSession()?.Connection?.Execute(
                $"INSERT INTO {InterviewsTableName} ({InterviewIdColumn}, {EntityIdColumn}, {RosterVectorColumn}, {EntityTypeColumn}, {InvalidValidationsColumn}) " +
                "VALUES(@InterviewId, @EntityId, @RosterVector, @EntityType, @InvalidValidations) " +
                "ON CONFLICT ON CONSTRAINT uk_interview " +
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
            => this.sessionProvider.GetSession()?.Connection?.Execute(
                $"INSERT INTO {InterviewsTableName} ({InterviewIdColumn}, {EntityIdColumn}, {RosterVectorColumn}, {EntityTypeColumn}, {EnabledColumn}) " +
                "VALUES(@InterviewId, @EntityId, @RosterVector, @EntityType, @Enabled) " +
                "ON CONFLICT ON CONSTRAINT uk_interview " +
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
            => this.sessionProvider.GetSession()?.Connection?.Execute(
                $"INSERT INTO {InterviewsTableName} ({InterviewIdColumn}, {EntityIdColumn}, {RosterVectorColumn}, {EntityTypeColumn}, {ReadOnlyColumn}) " +
                "VALUES(@InterviewId, @EntityId, @RosterVector, @EntityType, true) " +
                "ON CONFLICT ON CONSTRAINT uk_interview " +
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
            => this.sessionProvider.GetSession()?.Connection?.Execute(
                $"INSERT INTO {InterviewsTableName} ({InterviewIdColumn}, {EntityIdColumn}, {RosterVectorColumn}, {EntityTypeColumn}) " +
                $"VALUES(@InterviewId, @EntityId, @RosterVector, @EntityType);",
                rosterIds.Select(x => new
                {
                    InterviewId = interviewId,
                    EntityId = x.Id,
                    RosterVector = x.RosterVector.ToArray(),
                    EntityType = EntityType.Section
                }));

        public void RemoveRosters(Guid interviewId, Identity[] rosterIds)
        {
            var questionnaireId = this.summaryRepository.Query(_ => _.Where(x => x.InterviewId == interviewId)
                .Select(x => x.QuestionnaireIdentity).FirstOrDefault());
            var questionnaire =
                this.questionnaireStorage.GetQuestionnaireDocument(QuestionnaireIdentity.Parse(questionnaireId));
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

            this.sessionProvider.GetSession()?.Connection?.Execute($"DELETE FROM {InterviewsTableName} " +
                                                                   $"WHERE {InterviewIdColumn} = @InterviewId " +
                                                                   $"AND {EntityIdColumn} = @EntityId " +
                                                                   $"AND {RosterVectorColumn} = @RosterVector;",
                sqlParams);

        }

        public void RemoveAnswers(Guid interviewId, Identity[] questionIds)
            => this.sessionProvider.GetSession()?.Connection?.Execute(
                $"INSERT INTO {InterviewsTableName} ({InterviewIdColumn}, {EntityIdColumn}, {RosterVectorColumn}, {EntityTypeColumn}) " +
                "VALUES(@InterviewId, @EntityId, @RosterVector, @EntityType) " +
                "ON CONFLICT ON CONSTRAINT uk_interview " +
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