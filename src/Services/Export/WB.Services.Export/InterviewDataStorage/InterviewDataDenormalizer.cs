using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using WB.Services.Export.Events.Interview;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.InterviewDataStorage.InterviewDataExport;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Questionnaire.Services;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.InterviewDataStorage
{
    public class InterviewDataDenormalizer :
        IFunctionalHandler,
        IAsyncEventHandler<InterviewCreated>,
        IAsyncEventHandler<InterviewDeleted>,
        IAsyncEventHandler<InterviewHardDeleted>,
        IAsyncEventHandler<TextQuestionAnswered>,
        IAsyncEventHandler<NumericIntegerQuestionAnswered>,
        IAsyncEventHandler<NumericRealQuestionAnswered>,
        IAsyncEventHandler<TextListQuestionAnswered>,
        IAsyncEventHandler<MultipleOptionsLinkedQuestionAnswered>,
        IAsyncEventHandler<MultipleOptionsQuestionAnswered>,
        IAsyncEventHandler<SingleOptionQuestionAnswered>,
        IAsyncEventHandler<SingleOptionLinkedQuestionAnswered>,
        IAsyncEventHandler<AreaQuestionAnswered>,
        IAsyncEventHandler<AudioQuestionAnswered>,
        IAsyncEventHandler<DateTimeQuestionAnswered>,
        IAsyncEventHandler<GeoLocationQuestionAnswered>,
        IAsyncEventHandler<PictureQuestionAnswered>,
        IAsyncEventHandler<QRBarcodeQuestionAnswered>,
        IAsyncEventHandler<YesNoQuestionAnswered>,
        
        IAsyncEventHandler<AnswerRemoved>,
        IAsyncEventHandler<AnswersRemoved>,
        IAsyncEventHandler<QuestionsDisabled>,
        IAsyncEventHandler<QuestionsEnabled>,
        IAsyncEventHandler<AnswersDeclaredInvalid>,
        IAsyncEventHandler<AnswersDeclaredValid>,
        IAsyncEventHandler<VariablesChanged>,
        IAsyncEventHandler<VariablesDisabled>,
        IAsyncEventHandler<VariablesEnabled>,
        IAsyncEventHandler<RosterInstancesAdded>,
        IAsyncEventHandler<RosterInstancesRemoved>,
        IAsyncEventHandler<GroupsDisabled>,
        IAsyncEventHandler<GroupsEnabled>
    {

        private readonly ITenantContext tenantContext;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IMemoryCache memoryCache;
        private readonly IInterviewDataExportCommandBuilder commandBuilder;

        private readonly InterviewDataState state;

        public InterviewDataDenormalizer(ITenantContext tenantContext, IQuestionnaireStorage questionnaireStorage,
            IMemoryCache memoryCache, IInterviewDataExportCommandBuilder commandBuilder)
        {
            this.tenantContext = tenantContext;
            this.questionnaireStorage = questionnaireStorage;
            this.memoryCache = memoryCache;
            this.commandBuilder = commandBuilder;

            state = new InterviewDataState();
        }

        public Task Handle(PublishedEvent<InterviewCreated> @event, CancellationToken token = default)
        {
            return AddInterview(@event.EventSourceId, token);
        }

        public Task Handle(PublishedEvent<InterviewDeleted> @event, CancellationToken token = default)
        {
            return RemoveInterview(@event.EventSourceId, token);
        }

        public Task Handle(PublishedEvent<InterviewHardDeleted> @event, CancellationToken token = default)
        {
            return RemoveInterview(@event.EventSourceId, token);
        }

        public Task Handle(PublishedEvent<TextQuestionAnswered> @event, CancellationToken token = default)
        {
            return UpdateQuestionValue(
                interviewId: @event.EventSourceId,
                entityId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                value: @event.Event.Answer,
                valueType: NpgsqlDbType.Text, token: token);
        }

        public Task Handle(PublishedEvent<NumericIntegerQuestionAnswered> @event, CancellationToken token = default)
        {
            return UpdateQuestionValue(
                interviewId: @event.EventSourceId,
                entityId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                value: @event.Event.Answer,
                valueType: NpgsqlDbType.Integer, token: token);
        }

        public Task Handle(PublishedEvent<NumericRealQuestionAnswered> @event, CancellationToken token = default)
        {
            double answer = (double)@event.Event.Answer;
            return UpdateQuestionValue(
                interviewId: @event.EventSourceId,
                entityId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                value: double.IsNaN(answer) ? null : (object)answer,
                valueType: NpgsqlDbType.Double, token: token);
        }

        public Task Handle(PublishedEvent<TextListQuestionAnswered> @event, CancellationToken token = default)
        {
            var answer = @event.Event.Answers.Select(a => new InterviewTextListAnswer(a.Item1, a.Item2)).ToArray();
            return UpdateQuestionValue(
                interviewId: @event.EventSourceId,
                entityId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                value: SerializeToJson(answer),
                valueType: NpgsqlDbType.Json, token: token);
        }

        public Task Handle(PublishedEvent<MultipleOptionsLinkedQuestionAnswered> @event, CancellationToken token = default)
        {
            return UpdateQuestionValue(
                interviewId: @event.EventSourceId,
                entityId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                value: SerializeToJson(@event.Event.SelectedRosterVectors.Select(c => c.Select(i => (int)i).ToArray()).ToArray()),
                valueType: NpgsqlDbType.Jsonb,
                token: token);
        }

        public Task Handle(PublishedEvent<MultipleOptionsQuestionAnswered> @event, CancellationToken token = default)
        {
            return UpdateQuestionValue(
                interviewId: @event.EventSourceId,
                entityId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                value: @event.Event.SelectedValues.Select(c => (int)c).ToArray(),
                valueType: NpgsqlDbType.Array | NpgsqlDbType.Integer, token: token);
        }

        public Task Handle(PublishedEvent<SingleOptionQuestionAnswered> @event, CancellationToken token = default)
        {
            return UpdateQuestionValue(
                interviewId: @event.EventSourceId,
                entityId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                value: (int)@event.Event.SelectedValue,
                valueType: NpgsqlDbType.Integer, token: token);
        }

        public Task Handle(PublishedEvent<SingleOptionLinkedQuestionAnswered> @event, CancellationToken token = default)
        {
            return UpdateQuestionValue(
                interviewId: @event.EventSourceId,
                entityId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                value: @event.Event.SelectedRosterVector.Select(c => (int)c).ToArray(),
                valueType: NpgsqlDbType.Array | NpgsqlDbType.Integer, token: token);
        }

        public Task Handle(PublishedEvent<AreaQuestionAnswered> @event, CancellationToken token = default)
        {
            var area = new Area(@event.Event.Geometry, @event.Event.MapName, @event.Event.NumberOfPoints,
                @event.Event.AreaSize, @event.Event.Length, @event.Event.Coordinates, @event.Event.DistanceToEditor);
            return UpdateQuestionValue(
                interviewId: @event.EventSourceId,
                entityId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                value: SerializeToJson(area),
                valueType: NpgsqlDbType.Jsonb, token: token);
        }

        public Task Handle(PublishedEvent<AudioQuestionAnswered> @event, CancellationToken token = default)
        {
            var audioAnswer = AudioAnswer.FromString(@event.Event.FileName, @event.Event.Length);
            return UpdateQuestionValue(
                interviewId: @event.EventSourceId,
                entityId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                value: SerializeToJson(audioAnswer),
                valueType: NpgsqlDbType.Jsonb, token: token);
        }

        public Task Handle(PublishedEvent<DateTimeQuestionAnswered> @event, CancellationToken token = default)
        {
            return UpdateQuestionValue(
                interviewId: @event.EventSourceId,
                entityId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                value: @event.Event.Answer,
                valueType: NpgsqlDbType.Timestamp,
                token: token);
        }

        public Task Handle(PublishedEvent<GeoLocationQuestionAnswered> @event, CancellationToken token = default)
        {
            GeoPosition geoPosition = new GeoPosition(@event.Event.Latitude,
                @event.Event.Longitude,
                @event.Event.Accuracy,
                @event.Event.Altitude,
                @event.Event.Timestamp);
            return UpdateQuestionValue(
                interviewId: @event.EventSourceId,
                entityId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                value: SerializeToJson(geoPosition),
                valueType: NpgsqlDbType.Jsonb, token: token);
        }

        public Task Handle(PublishedEvent<PictureQuestionAnswered> @event, CancellationToken token = default)
        {
            return UpdateQuestionValue(
                interviewId: @event.EventSourceId,
                entityId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                value: @event.Event.PictureFileName,
                valueType: NpgsqlDbType.Text, token: token);
        }

        public Task Handle(PublishedEvent<QRBarcodeQuestionAnswered> @event, CancellationToken token = default)
        {
            return UpdateQuestionValue(
                interviewId: @event.EventSourceId,
                entityId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                value: @event.Event.Answer,
                valueType: NpgsqlDbType.Text, 
                token: token);
        }

        public Task Handle(PublishedEvent<YesNoQuestionAnswered> @event, CancellationToken token = default)
        {
            return UpdateQuestionValue(
                interviewId: @event.EventSourceId,
                entityId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                value: SerializeToJson(@event.Event.AnsweredOptions),
                valueType: NpgsqlDbType.Jsonb, 
                token: token);
        }

        public Task Handle(PublishedEvent<AnswerRemoved> @event, CancellationToken token = default)
        {
            return UpdateQuestionValue(
                interviewId: @event.EventSourceId,
                entityId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                value: null,
                valueType: NpgsqlDbType.Unknown, token: token);
        }

        public async Task Handle(PublishedEvent<AnswersRemoved> @event, CancellationToken token = default)
        {
            foreach (var question in @event.Event.Questions)
            {
                await UpdateQuestionValue(
                    interviewId: @event.EventSourceId,
                    entityId: question.Id,
                    rosterVector: question.RosterVector,
                    value: null,
                    valueType: NpgsqlDbType.Unknown, token: token);
            }
        }

        public async Task Handle(PublishedEvent<QuestionsDisabled> @event, CancellationToken token = default)
        {
            foreach (var question in @event.Event.Questions)
            {
                await UpdateEnablementValue(
                    interviewId: @event.EventSourceId,
                    entityId: question.Id,
                    rosterVector: question.RosterVector,
                    isEnabled: false, token: token);
            }
        }

        public async Task Handle(PublishedEvent<QuestionsEnabled> @event, CancellationToken token = default)
        {
            foreach (var question in @event.Event.Questions)
            {
                await UpdateEnablementValue(
                    interviewId: @event.EventSourceId,
                    entityId: question.Id,
                    rosterVector: question.RosterVector,
                    isEnabled: true, token: token);
            }
        }

        public async Task Handle(PublishedEvent<AnswersDeclaredInvalid> @event, CancellationToken token = default)
        {
            var failedValidationConditions = @event.Event.FailedValidationConditions;
            foreach (var question in @event.Event.Questions)
            {
                await UpdateValidityValue(
                    interviewId: @event.EventSourceId,
                    entityId: question.Id,
                    rosterVector: question.RosterVector,
                    validityValue: failedValidationConditions[question].Select(c => c.FailedConditionIndex).ToArray(), token: token); 
            }
        }

        public async Task Handle(PublishedEvent<AnswersDeclaredValid> @event, CancellationToken token = default)
        {
            foreach (var question in @event.Event.Questions)
            {
                await UpdateValidityValue(
                    interviewId: @event.EventSourceId,
                    entityId: question.Id,
                    rosterVector: question.RosterVector,
                    validityValue: null, token: token);
            }
        }

        public async Task Handle(PublishedEvent<VariablesChanged> @event, CancellationToken token = default)
        {
            foreach (var variable in @event.Event.ChangedVariables)
            {
                await UpdateVariableValue(
                    interviewId: @event.EventSourceId,
                    entityId: variable.Identity.Id,
                    rosterVector: variable.Identity.RosterVector,
                    value: variable.NewValue, 
                    token: token);
            }
        }

        public async Task Handle(PublishedEvent<VariablesDisabled> @event, CancellationToken token = default)
        {
            foreach (var identity in @event.Event.Variables)
            {
                await UpdateEnablementValue(
                    interviewId: @event.EventSourceId,
                    entityId: identity.Id,
                    rosterVector: identity.RosterVector,
                    isEnabled: false, token: token);
            }
        }

        public async Task Handle(PublishedEvent<VariablesEnabled> @event, CancellationToken token = default)
        {
            foreach (var identity in @event.Event.Variables)
            {
                await UpdateEnablementValue(
                    interviewId: @event.EventSourceId,
                    entityId: identity.Id,
                    rosterVector: identity.RosterVector,
                    isEnabled: true, token: token);
            }
        }

        public async Task Handle(PublishedEvent<RosterInstancesAdded> @event, CancellationToken token = default)
        {
            foreach (var rosterInstance in @event.Event.Instances)
            {
                var rosterVector = rosterInstance.OuterRosterVector.Append(rosterInstance.RosterInstanceId);
                await AddRoster(@event.EventSourceId, rosterInstance.GroupId, rosterVector, token);
            }
        }

        public async Task Handle(PublishedEvent<RosterInstancesRemoved> @event, CancellationToken token = default)
        {
            foreach (var rosterInstance in @event.Event.Instances)
            {
                var rosterVector = rosterInstance.OuterRosterVector.Append(rosterInstance.RosterInstanceId).ToArray();
                await RemoveRoster(@event.EventSourceId, rosterInstance.GroupId, rosterVector, token);
            }
        }

        public async Task Handle(PublishedEvent<GroupsDisabled> @event, CancellationToken token = default)
        {
            foreach (var groupId in @event.Event.Groups)
            {
                await UpdateEnablementValue(
                    interviewId: @event.EventSourceId,
                    entityId: groupId.Id,
                    rosterVector: groupId.RosterVector,
                    isEnabled: false, 
                    token: token);
            }
        }

        public async Task Handle(PublishedEvent<GroupsEnabled> @event, CancellationToken token = default)
        {
            foreach (var groupId in @event.Event.Groups)
            {
                await UpdateEnablementValue(
                    interviewId: @event.EventSourceId,
                    entityId: groupId.Id,
                    rosterVector: groupId.RosterVector,
                    isEnabled: true,
                    token: token);
            }
        }

        private async Task AddInterview(Guid interviewId, CancellationToken token = default)
        {
            var questionnaire = await GetQuestionnaireByInterviewIdAsync(interviewId, token);
            if (questionnaire == null)
                return;

            var topLevelGroups = questionnaire.GetInterviewLevelGroupsWithQuestionOrVariables();
            foreach (var topLevelGroup in topLevelGroups)
            {
                state.InsertInterviewInTable(topLevelGroup.TableName, interviewId);
                state.InsertInterviewInTable(topLevelGroup.EnablementTableName, interviewId);
                state.InsertInterviewInTable(topLevelGroup.ValidityTableName, interviewId);
            }
        }

        private async Task AddRoster(Guid interviewId, Guid groupId, RosterVector rosterVector, CancellationToken token = default)
        {
            var questionnaire = await GetQuestionnaireByInterviewIdAsync(interviewId, token);
            if (questionnaire == null)
                return;

            var @group = questionnaire.Find<Group>(groupId);
            state.InsertRosterInTable(@group.TableName, interviewId, rosterVector);
            state.InsertRosterInTable(@group.EnablementTableName, interviewId, rosterVector);

            if (group.Children.Any(c => c is Question))
                state.InsertRosterInTable(@group.ValidityTableName, interviewId, rosterVector);
        }

        public async Task UpdateQuestionValue(Guid interviewId, Guid entityId, RosterVector rosterVector, object value, NpgsqlDbType valueType, CancellationToken token = default)
        {
            var questionnaire = await GetQuestionnaireByInterviewIdAsync(interviewId, token);
            if (questionnaire == null)
                return;

            var entity = questionnaire.Find<IQuestionnaireEntity>(entityId);
            var parentGroup = (Group)entity.GetParent();
            var columnName = ((Question)entity).ColumnName;
            state.UpdateValueInTable(parentGroup.TableName, interviewId, rosterVector, columnName, value, valueType);
        }

        public async Task UpdateVariableValue(Guid interviewId, Guid entityId, RosterVector rosterVector, object value, CancellationToken token = default)
        {
            var questionnaire = await GetQuestionnaireByInterviewIdAsync(interviewId, token);
            if (questionnaire == null)
                return;

            var variable = questionnaire.Find<Variable>(entityId);
            var parentGroup = (Group)variable.GetParent();
            var columnName = variable.ColumnName;
            var columnType = GetPostgresSqlTypeForVariable(variable);

            if (columnType == NpgsqlDbType.Double && value is string sValue && sValue == "NaN")
                value = double.NaN;

            state.UpdateValueInTable(parentGroup.TableName, interviewId, rosterVector, columnName, value, columnType);
        }

        public async Task UpdateEnablementValue(Guid interviewId, Guid entityId, RosterVector rosterVector, bool isEnabled, CancellationToken token = default)
        {
            var questionnaire = await GetQuestionnaireByInterviewIdAsync(interviewId, token);
            if (questionnaire == null)
                return;

            var entity = questionnaire.Find<IQuestionnaireEntity>(entityId);

            if (entity is Group group && !group.IsRoster && !group.HasAnyExportableQuestions)
                return;

            var tableName = ResolveGroupForEnablement(entity).EnablementTableName;
            var columnName = ResolveColumnNameForEnablement(entity);
            state.UpdateValueInTable(tableName, interviewId, rosterVector, columnName, isEnabled, NpgsqlDbType.Boolean);
        }
        
        public async Task UpdateValidityValue(Guid interviewId, Guid entityId, RosterVector rosterVector, int[] validityValue, CancellationToken token = default)
        {
            var questionnaire = await GetQuestionnaireByInterviewIdAsync(interviewId, token);
            if (questionnaire == null)
                return;
            var entity = questionnaire.Find<Question>(entityId);
            var tableName = ((Group)entity.GetParent()).ValidityTableName;
            var columnName = entity.ColumnName;
            state.UpdateValueInTable(tableName, interviewId, rosterVector, columnName, validityValue, NpgsqlDbType.Array | NpgsqlDbType.Integer);
        }

        public async Task RemoveRoster(Guid interviewId, Guid groupId, RosterVector rosterVector, CancellationToken token = default)
        {
            var questionnaire = await GetQuestionnaireByInterviewIdAsync(interviewId, token);
            if (questionnaire == null)
                return;

            var @group = questionnaire.Find<Group>(groupId);
            state.RemoveRosterFromTable(@group.TableName, interviewId, rosterVector);
            state.RemoveRosterFromTable(@group.EnablementTableName, interviewId, rosterVector);

            if (group.Children.Any(c => c is Question))
                state.RemoveRosterFromTable(@group.ValidityTableName, interviewId, rosterVector);
        }

        public async Task RemoveInterview(Guid interviewId, CancellationToken token = default)
        {
            var questionnaire = await GetQuestionnaireByInterviewIdAsync(interviewId, token);
            if (questionnaire == null)
                return;

            var topLevelGroups = questionnaire.GetInterviewLevelGroupsWithQuestionOrVariables();
            foreach (var topLevelGroup in topLevelGroups)
            {
                state.RemoveInterviewFromTable(topLevelGroup.TableName, interviewId);
                state.RemoveInterviewFromTable(topLevelGroup.EnablementTableName, interviewId);
                state.RemoveInterviewFromTable(topLevelGroup.ValidityTableName, interviewId);
            }
        }

        public async Task SaveStateAsync(CancellationToken cancellationToken)
        {
            var commands = GenerateSqlCommandsAsync();
            foreach (var sqlCommand in commands)
            {
                sqlCommand.Connection = tenantContext.DbContext.Database.GetDbConnection();
                await sqlCommand.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        private List<DbCommand> GenerateSqlCommandsAsync()
        {
            var commands = new List<DbCommand>();

            foreach (var tableWithAddInterviews in state.GetInsertInterviewsData())
                commands.Add(commandBuilder.CreateInsertCommandForTable(tableWithAddInterviews.TableName, tableWithAddInterviews.InterviewIds));

            foreach (var tableWithAddRosters in state.GetInsertRostersData())
                commands.Add(commandBuilder.CreateAddRosterInstanceForTable(tableWithAddRosters.TableName, tableWithAddRosters.RosterLevelInfo));

            foreach (var updateValueInfo in state.GetUpdateValuesData())
            {
                var updateValueCommand = commandBuilder.CreateUpdateValueForTable(updateValueInfo.TableName,
                    updateValueInfo.RosterLevelInfo,
                    updateValueInfo.UpdateValuesInfo);
                commands.Add(updateValueCommand);
            }

            foreach (var tableWithRemoveRosters in state.GetRemoveRostersData())
                commands.Add(commandBuilder.CreateRemoveRosterInstanceForTable(tableWithRemoveRosters.TableName, tableWithRemoveRosters.RosterLevelInfo));

            foreach (var tableWithRemoveInterviews in state.GetRemoveInterviewsData())
                commands.Add(commandBuilder.CreateDeleteCommandForTable(tableWithRemoveInterviews.TableName, tableWithRemoveInterviews.InterviewIds));

            return commands;
        }

        private async Task<QuestionnaireDocument> GetQuestionnaireByInterviewIdAsync(Guid interviewId, CancellationToken token = default)
        {
            var key = $"{nameof(InterviewDataDenormalizer)}:{tenantContext.Tenant.Name}:{interviewId}";
            var questionnaireId = await memoryCache.GetOrCreateAsync(key,
                async entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromMinutes(3);
                    var interviewSummary = await this.tenantContext.DbContext.InterviewReferences.FindAsync(interviewId);
                    if (interviewSummary == null)
                        return null;
                    return new QuestionnaireId(interviewSummary.QuestionnaireId);
                });

            if (questionnaireId == null)
                return null;
            var questionnaire = await questionnaireStorage.GetQuestionnaireAsync(tenantContext.Tenant, questionnaireId, token);
            return questionnaire;
        }

        private Group ResolveGroupForEnablement(IQuestionnaireEntity entity)
        {
            return (entity as Group) ?? ((Group)entity.GetParent());
        }

        private string ResolveColumnNameForEnablement(IQuestionnaireEntity entity)
        {
            switch (entity)
            {
                case Group group:
                    return InterviewDatabaseConstants.InstanceValue;
                case Question question:
                    return question.ColumnName;
                case Variable variable:
                    return variable.ColumnName;
                default:
                    throw new ArgumentException("Unsupported entity type: " + entity.GetType().Name);
            }
        }

        private NpgsqlDbType GetPostgresSqlTypeForVariable(Variable variable)
        {
            switch (variable.Type)
            {
                case VariableType.Boolean: return NpgsqlDbType.Boolean;
                case VariableType.DateTime: return NpgsqlDbType.Timestamp;
                case VariableType.Double: return NpgsqlDbType.Double;
                case VariableType.LongInteger: return NpgsqlDbType.Bigint;
                case VariableType.String: return NpgsqlDbType.Text;
                default:
                    throw new ArgumentException("Unknown variable type: " + variable.Type);
            }
        }

        private string SerializeToJson(object value) => JsonConvert.SerializeObject(value);
    }
}
