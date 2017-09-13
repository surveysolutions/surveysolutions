using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Main.Core.Entities.SubEntities;
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

        private const string AsIntColumn = "anint";
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

        private readonly IPlainStorageAccessor<InterviewDbEntity> entitiesRepository;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> summaryRepository;
        private readonly IDbTransaction dbTransaction;
        private readonly IQuestionnaireStorage questionnaireStorage;

        public InterviewFactory(IPlainStorageAccessor<InterviewDbEntity> interviewEntitiesRepository, 
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryRepository,
            IQuestionnaireStorage questionnaireStorage,
            IDbTransaction dbTransaction)
        {
            this.entitiesRepository = interviewEntitiesRepository;
            this.summaryRepository = interviewSummaryRepository;
            this.dbTransaction = dbTransaction;
            this.questionnaireStorage = questionnaireStorage;
        }

        public Identity[] GetFlaggedQuestionIds(Guid interviewId) => this.entitiesRepository.Query(_
            => _.Where(x => x.InterviewId == interviewId && x.HasFlag).Select(x => x.Identity).ToArray());

        public void SetFlagToQuestion(Guid interviewId, Identity questionIdentity, bool flagged)
        {
            this.ThrowIfInterviewDeletedOrReadOnly(interviewId);

            this.dbTransaction.Connection.Execute(
                $"INSERT INTO {InterviewsTableName} ({InterviewIdColumn}, {EntityIdColumn}, {RosterVectorColumn}, {EntityTypeColumn}, {FlagColumn}) " +
                $"VALUES(@InterviewId, @EntityId, @RosterVector, @EntityType, {flagged}) " +
                "ON CONFLICT ON CONSTRAINT uk_interview " +
                "DO UPDATE SET " +
                $"{FlagColumn} = true;",
                new
                {
                    InterviewId = interviewId,
                    EntityId = questionIdentity.Id,
                    RosterVector = questionIdentity.RosterVector.ToArray(),
                    EntityType = EntityType.Question
                }, this.dbTransaction);
        }

        public void RemoveInterview(Guid interviewId)
            => this.dbTransaction.Connection.Execute($"DELETE FROM {InterviewsTableName} WHERE {InterviewIdColumn} = @InterviewId",
                new[] {new {InterviewId = interviewId}}, this.dbTransaction);

        public void UpdateAnswer(Guid interviewId, Identity questionIdentity, object answer)
            => UpdateAnswer(interviewId, questionIdentity, answer, EntityType.Question);

        public void UpdateVariables(Guid interviewId, ChangedVariable[] variables) 
            => variables.ForEach(variable => this.UpdateAnswer(interviewId, variable.Identity, variable.NewValue, EntityType.Variable));

        private void UpdateAnswer(Guid interviewId, Identity questionIdentity, object answer, EntityType entityType)
            => this.dbTransaction.Connection.Execute(
                $"INSERT INTO {InterviewsTableName} ({InterviewIdColumn}, {EntityIdColumn}, {RosterVectorColumn}, {EntityTypeColumn}, {AnswerTypeColumn}, {this.GetColumnNameByAnswer(answer)}) " +
                "VALUES(@InterviewId, @EntityId, @RosterVector, @EntityType, @AnswerType, @Answer) " +
                "ON CONFLICT ON CONSTRAINT uk_interview " +
                "DO UPDATE SET " +
                $"{this.GetColumnNameByAnswer(answer)} = @Answer;",
                new
                {
                    InterviewId = interviewId,
                    EntityId = questionIdentity.Id,
                    RosterVector = questionIdentity.RosterVector.ToArray(),
                    EntityType = entityType,
                    AnswerType = InterviewDbEntity.GetAnswerType(answer),
                    Answer = answer
                }, this.dbTransaction);

        public void MakeEntitiesValid(Guid interviewId, Identity[] entityIds, EntityType entityType)
            => this.dbTransaction.Connection.Execute(
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
                }), this.dbTransaction);

        public void MakeEntitiesInvalid(Guid interviewId, IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>> entityIds, EntityType entityType)
            => this.dbTransaction.Connection.Execute(
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
                }), this.dbTransaction);

        public void EnableEntities(Guid interviewId, Identity[] entityIds, EntityType entityType, bool isEnabled)
            => this.dbTransaction.Connection.Execute(
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
                }), this.dbTransaction);

        public void MarkQuestionsAsReadOnly(Guid interviewId, Identity[] questionIds)
            => this.dbTransaction.Connection.Execute(
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
                }), this.dbTransaction);

        public void AddRosters(Guid interviewId, Identity[] rosterIds)
            => this.dbTransaction.Connection.Execute(
                $"INSERT INTO {InterviewsTableName} ({InterviewIdColumn}, {EntityIdColumn}, {RosterVectorColumn}, {EntityIdColumn}) " +
                $"VALUES(@InterviewId, @EntityId, @RosterVector, @EntityType);",
                rosterIds.Select(x => new
                {
                    InterviewId = interviewId,
                    EntityId = x.Id,
                    x.RosterVector,
                    EntityType = EntityType.Section
                }), this.dbTransaction);

        public void RemoveRosters(Guid interviewId, Identity[] rosterIds)
        {
            var questionnaireId = this.summaryRepository.Query(_=>_.Where(x=>x.InterviewId == interviewId).Select(x=>x.QuestionnaireIdentity).FirstOrDefault());
            var questionnarie = this.questionnaireStorage.GetQuestionnaireDocument(QuestionnaireIdentity.Parse(questionnaireId));
            if (questionnarie == null) return;


            var removedEntityIds = new List<Identity>();
            foreach (var instance in rosterIds)
            {
                var roster = questionnarie.Find<IGroup>(instance.Id);

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

            this.dbTransaction.Connection.Execute($"DELETE FROM {InterviewsTableName} " +
                                       $"WHERE {InterviewIdColumn} = @InterviewId " +
                                       $"AND {EntityIdColumn} = @EntityId " +
                                       $"AND {RosterVectorColumn} = @RosterVector;", sqlParams, this.dbTransaction);
        
        }

        public void RemoveAnswers(Guid interviewId, Identity[] questionIds)
            => this.dbTransaction.Connection.Execute(
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
                }), this.dbTransaction);

        private string GetColumnNameByAnswer(object answer)
        {
            switch (InterviewDbEntity.GetAnswerType(answer))
            {
                case AnswerType.Area:
                    return AsAreaColumn;
                case AnswerType.Audio:
                    return AsAudioColumn;
                case AnswerType.Bool:
                    return AsBoolColumn;
                case AnswerType.Datetime:
                    return AsDateTimeColumn;
                case AnswerType.Double:
                    return AsDoubleColumn;
                case AnswerType.Gps:
                    return AsGpsColumn;
                case AnswerType.Int:
                    return AsIntColumn;
                case AnswerType.IntArray:
                    return AsIntArrayColumn;
                case AnswerType.IntMatrix:
                    return AsIntMatrixColumn;
                case AnswerType.Long:
                    return AsLongColumn;
                case AnswerType.String:
                    return AsStringColumn;
                case AnswerType.TextList:
                    return AsListColumn;
                case AnswerType.YesNoList:
                    return AsYesNoColumn;
                case null:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return null;
        }

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