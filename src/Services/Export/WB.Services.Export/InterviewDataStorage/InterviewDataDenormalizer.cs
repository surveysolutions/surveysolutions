using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
        IAsyncEventHandler<InterviewFromPreloadedDataCreated>,
        IAsyncEventHandler<InterviewOnClientCreated>,
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
        IAsyncEventHandler<GroupsEnabled>,
        IAsyncEventHandler<StaticTextsEnabled>,
        IAsyncEventHandler<StaticTextsDisabled>,
        IAsyncEventHandler<StaticTextsDeclaredValid>,
        IAsyncEventHandler<StaticTextsDeclaredInvalid>
    {

        private readonly ITenantContext tenantContext;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IMemoryCache memoryCache;
        private readonly IInterviewDataExportBulkCommandBuilder commandBuilder;
        private readonly ILogger<InterviewDataDenormalizer> logger;
        private readonly IInterviewReferencesStorage interviewReferencesStorage;
        private readonly ICommandExecutor commandExecutor;

        private readonly InterviewDataState state;

        public InterviewDataDenormalizer(ITenantContext tenantContext,
            IQuestionnaireStorage questionnaireStorage,
            IMemoryCache memoryCache,
            IInterviewDataExportBulkCommandBuilder commandBuilder,
            ILogger<InterviewDataDenormalizer> logger,
            IInterviewReferencesStorage interviewReferencesStorage,
            ICommandExecutor commandExecutor)
        {
            this.tenantContext = tenantContext;
            this.questionnaireStorage = questionnaireStorage;
            this.memoryCache = memoryCache;
            this.commandBuilder = commandBuilder;
            this.logger = logger;
            this.interviewReferencesStorage = interviewReferencesStorage;
            this.commandExecutor = commandExecutor;

            state = new InterviewDataState();
        }

        public Task Handle(PublishedEvent<InterviewCreated> @event, CancellationToken token = default)
        {
            return AddInterview(@event.EventSourceId, token);
        }

        public Task Handle(PublishedEvent<InterviewFromPreloadedDataCreated> @event, CancellationToken token = default)
        {
            return AddInterview(@event.EventSourceId, token);
        }

        public Task Handle(PublishedEvent<InterviewOnClientCreated> @event, CancellationToken token = default)
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

        public async Task Handle(PublishedEvent<DateTimeQuestionAnswered> @event, CancellationToken token = default)
        {
            await UpdateQuestionValue(
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
            var failedConditionsStorage = @event.Event.FailedConditionsStorage;
            foreach (var validationCondition in failedConditionsStorage)
            {
                await UpdateValidityValue(
                    interviewId: @event.EventSourceId,
                    entityId: validationCondition.Key.Id,
                    rosterVector: validationCondition.Key.RosterVector,
                    validityValue: validationCondition.Value?.Select(c => c.FailedConditionIndex).ToArray() ?? new []{ 0 }, 
                    token: token); 
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
                    validityValue: null, 
                    token: token);
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

        public async Task Handle(PublishedEvent<StaticTextsEnabled> @event, CancellationToken token = default)
        {
            foreach (var identity in @event.Event.StaticTexts)
            {
                await UpdateEnablementValue(
                    interviewId: @event.EventSourceId,
                    entityId: identity.Id,
                    rosterVector: identity.RosterVector,
                    isEnabled: true,
                    token: token);
            }
        }

        public async Task Handle(PublishedEvent<StaticTextsDisabled> @event, CancellationToken token = default)
        {
            foreach (var identity in @event.Event.StaticTexts)
            {
                await UpdateEnablementValue(
                    interviewId: @event.EventSourceId,
                    entityId: identity.Id,
                    rosterVector: identity.RosterVector,
                    isEnabled: false,
                    token: token);
            }
        }

        public async Task Handle(PublishedEvent<StaticTextsDeclaredValid> @event, CancellationToken token = default)
        {
            foreach (var staticTextId in @event.Event.StaticTexts)
            {
                await UpdateValidityValue(
                    interviewId: @event.EventSourceId,
                    entityId: staticTextId.Id,
                    rosterVector: staticTextId.RosterVector,
                    validityValue: null,
                    token: token);
            }
        }

        public async Task Handle(PublishedEvent<StaticTextsDeclaredInvalid> @event, CancellationToken token = default)
        {
            var failedValidationConditions = @event.Event.FailedValidationConditions;
            if (failedValidationConditions != null)
            {
                foreach (var keyValuePair in failedValidationConditions)
                {
                    await UpdateValidityValue(
                        interviewId: @event.EventSourceId,
                        entityId: keyValuePair.Key.Id,
                        rosterVector: keyValuePair.Key.RosterVector,
                        validityValue: keyValuePair.Value.Select(c => c.FailedConditionIndex).ToArray(),
                        token: token);
                }
            }
        }

        private async Task AddInterview(Guid interviewId, CancellationToken token = default)
        {
            var questionnaire = await GetQuestionnaireByInterviewIdAsync(interviewId, token);
            if (questionnaire == null)
                return;

            var levelTables = questionnaire.DatabaseStructure.GetLevelTables(questionnaire.PublicKey);
            foreach (var levelTable in levelTables)
            {
                state.InsertInterviewInTable(levelTable.TableName, interviewId);
                state.InsertInterviewInTable(levelTable.EnablementTableName, interviewId);
                state.InsertInterviewInTable(levelTable.ValidityTableName, interviewId);
            }
        }

        private async Task AddRoster(Guid interviewId, Guid groupId, RosterVector rosterVector, CancellationToken token = default)
        {
            var questionnaire = await GetQuestionnaireByInterviewIdAsync(interviewId, token);
            if (questionnaire == null)
                return;

            var levelTables = questionnaire.DatabaseStructure.GetLevelTables(groupId);
            foreach (var levelTable in levelTables)
            {
                state.InsertRosterInTable(levelTable.TableName, interviewId, rosterVector);
                state.InsertRosterInTable(levelTable.EnablementTableName, interviewId, rosterVector);
                state.InsertRosterInTable(levelTable.ValidityTableName, interviewId, rosterVector);
            }
        }

        public async Task UpdateQuestionValue(Guid interviewId, Guid entityId, RosterVector rosterVector, object? value, NpgsqlDbType valueType, CancellationToken token = default)
        {
            var questionnaire = await GetQuestionnaireByInterviewIdAsync(interviewId, token);
            if (questionnaire == null)
                return;

            var question = questionnaire.Find<Question>(entityId);
            if (question == null)
                return;

            var tableName = questionnaire.DatabaseStructure.GetDataTableName(entityId);
            var columnName = question.ColumnName;
            state.UpdateValueInTable(tableName, interviewId, rosterVector, columnName, value, valueType);
        }

        public async Task UpdateVariableValue(Guid interviewId, Guid entityId, RosterVector rosterVector, object value, CancellationToken token = default)
        {
            var questionnaire = await GetQuestionnaireByInterviewIdAsync(interviewId, token);
            if (questionnaire == null)
                return;

            var variable = questionnaire.Find<Variable>(entityId);

            if (variable == null || !variable.IsExportable) return;

            var tableName = questionnaire.DatabaseStructure.GetDataTableName(entityId);
            var columnName = variable.ColumnName;
            var columnType = GetPostgresSqlTypeForVariable(variable);

            if (columnType == NpgsqlDbType.Double && value is string stringValue)
                value = Double.Parse(stringValue, CultureInfo.InvariantCulture);

            if (columnType == NpgsqlDbType.Double && !(value is double))
            {
                if (value is BigInteger)
                    value = double.Parse(value.ToString());
                else
                    value = Convert.ToDouble(value);
            }

            if (columnType == NpgsqlDbType.Bigint && !(value is long))
            {
                value = Convert.ToInt64(value);
            }

            state.UpdateValueInTable(tableName, interviewId, rosterVector, columnName, value, columnType);
        }

        public async Task UpdateEnablementValue(Guid interviewId, Guid entityId, RosterVector rosterVector, bool isEnabled, CancellationToken token = default)
        {
            var questionnaire = await GetQuestionnaireByInterviewIdAsync(interviewId, token);
            if (questionnaire == null)
                return;

            var entity = questionnaire.Find<IQuestionnaireEntity>(entityId);
            
            if (entity == null || !entity.IsExportable) return;

            var tableName = questionnaire.DatabaseStructure.GetEnablementDataTableName(entityId);
            var columnName = ResolveColumnNameForEnablement(entity);
            state.UpdateValueInTable(tableName, interviewId, rosterVector, columnName, isEnabled, NpgsqlDbType.Boolean);
        }
        
        public async Task UpdateValidityValue(Guid interviewId, Guid entityId, RosterVector rosterVector, int[]? validityValue, CancellationToken token = default)
        {
            var questionnaire = await GetQuestionnaireByInterviewIdAsync(interviewId, token);
            if (questionnaire == null)
                return;

            var entity = questionnaire.Find<IQuestionnaireEntity>(entityId);
            if (entity == null || !entity.IsExportable) return;

            var tableName = questionnaire.DatabaseStructure.GetValidityDataTableName(entityId);
            var columnName = (entity as Question)?.ColumnName 
                             ?? (entity as StaticText)?.ColumnName 
                             ?? throw new ArgumentException("Does not support this entity type: " + entity.GetType().Name);

            state.UpdateValueInTable(tableName, interviewId, rosterVector, columnName, validityValue, NpgsqlDbType.Array | NpgsqlDbType.Integer);
        }

        public async Task RemoveRoster(Guid interviewId, Guid groupId, RosterVector rosterVector, CancellationToken token = default)
        {
            var questionnaire = await GetQuestionnaireByInterviewIdAsync(interviewId, token);
            if (questionnaire == null)
                return;

            var levelTables = questionnaire.DatabaseStructure.GetLevelTables(groupId);
            foreach (var levelTable in levelTables)
            {
                state.RemoveRosterFromTable(levelTable.TableName, interviewId, rosterVector);
                state.RemoveRosterFromTable(levelTable.EnablementTableName, interviewId, rosterVector);
                state.RemoveRosterFromTable(levelTable.ValidityTableName, interviewId, rosterVector);
            }
        }

        public async Task RemoveInterview(Guid interviewId, CancellationToken token = default)
        {
            var questionnaire = await GetQuestionnaireByInterviewIdAsync(interviewId, token);
            if (questionnaire == null)
                return;

            var levelTables = questionnaire.DatabaseStructure.GetAllLevelTables();
            foreach (var levelTable in levelTables)
            {
                state.RemoveInterviewFromTable(levelTable.TableName, interviewId);
                state.RemoveInterviewFromTable(levelTable.EnablementTableName, interviewId);
                state.RemoveInterviewFromTable(levelTable.ValidityTableName, interviewId);
            }
        }

        public async Task SaveStateAsync(CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            
            await using var command = commandBuilder.BuildCommandsInExecuteOrderFromState(state);
            logger.LogDebug("Save state command with {parameters} parameters generated in {time}.", command.Parameters.Count, sw.Elapsed);
            sw.Restart();

            if (string.IsNullOrWhiteSpace(command.CommandText))
            {
                logger.LogWarning("Save state is not applied. Nothing to apply.", sw.Elapsed);
                return;
            }
            
            await commandExecutor.ExecuteNonQueryAsync(command, cancellationToken);

            logger.LogDebug("Save state command applied on DB in {time}", sw.Elapsed);
        }

        private Guid? lastInterviewId = null;
        private QuestionnaireDocument? lastQuestionnaire = null;

        private async ValueTask<QuestionnaireDocument?> GetQuestionnaireByInterviewIdAsync(Guid interviewId, CancellationToken token = default)
        {
            if (lastInterviewId == interviewId && lastQuestionnaire != null) return lastQuestionnaire;

            var key = $"{nameof(InterviewDataDenormalizer)}:{tenantContext.Tenant.Name}:{interviewId}";
            var questionnaireId = await memoryCache.GetOrCreateAsync(key,
                async entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromMinutes(3);
                    var interviewSummary = await interviewReferencesStorage.FindAsync(interviewId);

                    if (interviewSummary == null)
                        return null;

                    return new QuestionnaireId(interviewSummary.QuestionnaireId);
                });

            if (questionnaireId == null)
            {
                lastQuestionnaire = null;
                return null;
            }

            var questionnaire = await questionnaireStorage.GetQuestionnaireAsync(questionnaireId, token: token);
            lastQuestionnaire = questionnaire;
            lastInterviewId = interviewId;
            return questionnaire;
        }

        private string ResolveColumnNameForEnablement(IQuestionnaireEntity entity)
        {
            switch (entity)
            {
                case Group group:
                    return group.ColumnName;
                case Question question:
                    return question.ColumnName;
                case Variable variable:
                    return variable.ColumnName;
                case StaticText staticText:
                    return staticText.ColumnName;
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

        private string SerializeToJson(object? value) => JsonConvert.SerializeObject(value);
    }
}
