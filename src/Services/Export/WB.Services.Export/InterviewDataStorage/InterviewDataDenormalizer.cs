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
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Questionnaire.Services;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.InterviewDataStorage
{
    public class RosterInfo
    {
        public Guid InterviewId { get; set; }
        public RosterVector RosterVector { get; set; }
    }

    public class UpdateValueInfo
    {
        public string ColumnName { get; set; }
        public object Value { get; set; }
        public NpgsqlDbType ValueType { get; set; }
    }

    public class InterviewDataState
    {
        public Dictionary<string, HashSet<Guid>> InsertInterviews { get; set; } = new Dictionary<string, HashSet<Guid>>();
        public Dictionary<string, HashSet<RosterInfo>> InsertRosters { get; set; } = new Dictionary<string, HashSet<RosterInfo>>();
        public Dictionary<string, Dictionary<RosterInfo, HashSet<UpdateValueInfo>>> UpdateValues = new Dictionary<string, Dictionary<RosterInfo, HashSet<UpdateValueInfo>>>();
        public Dictionary<string, HashSet<RosterInfo>> RemoveRosters { get; set; } = new Dictionary<string, HashSet<RosterInfo>>();
        public Dictionary<string, HashSet<Guid>> RemoveInterviews { get; set; } = new Dictionary<string, HashSet<Guid>>();

        public void InsertInterviewInTable(string tableName, Guid interviewId)
        {
            if (!InsertInterviews.ContainsKey(tableName))
                InsertInterviews.Add(tableName, new HashSet<Guid>());
            InsertInterviews[tableName].Add(interviewId);
        }

        public void RemoveInterviewFromTable(string tableName, Guid interviewId)
        {
            if (!RemoveInterviews.ContainsKey(tableName))
                RemoveInterviews.Add(tableName, new HashSet<Guid>());
            RemoveInterviews[tableName].Add(interviewId);
        }

        public void InsertRosterInTable(string tableName, Guid interviewId, RosterVector rosterVector)
        {
            if (!InsertRosters.ContainsKey(tableName))
                InsertRosters.Add(tableName, new HashSet<RosterInfo>());
            InsertRosters[tableName].Add(new RosterInfo() { InterviewId = interviewId, RosterVector = rosterVector});
        }

        public void RemoveRosterFromTable(string tableName, Guid interviewId, RosterVector rosterVector)
        {
            if (!RemoveRosters.ContainsKey(tableName))
                RemoveRosters.Add(tableName, new HashSet<RosterInfo>());
            RemoveRosters[tableName].Add(new RosterInfo() { InterviewId = interviewId, RosterVector = rosterVector });
        }

        public void UpdateValueInTable(string tableName, Guid interviewId, RosterVector rosterVector, string columnName, object value, NpgsqlDbType valueType)
        {
            if (!UpdateValues.ContainsKey(tableName))
                UpdateValues.Add(tableName, new Dictionary<RosterInfo, HashSet<UpdateValueInfo>>());
            var updateValueForTable = UpdateValues[tableName];
            var rosterInfo = new RosterInfo() {InterviewId = interviewId, RosterVector = rosterVector};
            if (!updateValueForTable.ContainsKey(rosterInfo))
                updateValueForTable.Add(rosterInfo, new HashSet<UpdateValueInfo>());
            updateValueForTable[rosterInfo].Add(new UpdateValueInfo() { ColumnName = columnName, Value = value, ValueType = valueType});
        }
    }


    public class InterviewDataDenormalizer :
        IFunctionalHandler,
        IEventHandler<InterviewCreated>,
        IEventHandler<InterviewHardDeleted>,

        IEventHandler<TextQuestionAnswered>,
        IEventHandler<NumericIntegerQuestionAnswered>,
        IEventHandler<NumericRealQuestionAnswered>,
        IEventHandler<TextListQuestionAnswered>,
        IEventHandler<MultipleOptionsLinkedQuestionAnswered>,
        IEventHandler<MultipleOptionsQuestionAnswered>,
        IEventHandler<SingleOptionQuestionAnswered>,
        IEventHandler<SingleOptionLinkedQuestionAnswered>,
        IEventHandler<AreaQuestionAnswered>,
        IEventHandler<AudioQuestionAnswered>,
        IEventHandler<DateTimeQuestionAnswered>,
        IEventHandler<GeoLocationQuestionAnswered>,
        IEventHandler<PictureQuestionAnswered>,
        IEventHandler<QRBarcodeQuestionAnswered>,
        IEventHandler<YesNoQuestionAnswered>,

        IEventHandler<AnswerRemoved>,
        IEventHandler<AnswersRemoved>,

        IEventHandler<QuestionsDisabled>,
        IEventHandler<QuestionsEnabled>,
        IEventHandler<AnswersDeclaredInvalid>,
        IEventHandler<AnswersDeclaredValid>,

        IEventHandler<VariablesChanged>,
        IEventHandler<VariablesDisabled>,
        IEventHandler<VariablesEnabled>,

        IEventHandler<RosterInstancesAdded>,
        IEventHandler<RosterInstancesRemoved>,
        IEventHandler<GroupsDisabled>,
        IEventHandler<GroupsEnabled>
    {

        private readonly ITenantContext tenantContext;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IMemoryCache memoryCache;

        private readonly InterviewDataState state;

        public InterviewDataDenormalizer(ITenantContext tenantContext, IQuestionnaireStorage questionnaireStorage,
            IMemoryCache memoryCache)
        {
            this.tenantContext = tenantContext;
            this.questionnaireStorage = questionnaireStorage;
            this.memoryCache = memoryCache;

            state = new InterviewDataState();
        }

        public Task Handle(PublishedEvent<InterviewCreated> @event)
        {
            return AddInterview(@event.EventSourceId);
        }

        public Task Handle(PublishedEvent<InterviewHardDeleted> @event)
        {
            return RemoveInterview(@event.EventSourceId);
        }

        public Task Handle(PublishedEvent<TextQuestionAnswered> @event)
        {
            return UpdateQuestionValue(
                interviewId: @event.EventSourceId,
                entityId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                value: @event.Event.Answer,
                valueType: NpgsqlDbType.Text
            );
        }

        public Task Handle(PublishedEvent<NumericIntegerQuestionAnswered> @event)
        {
            return UpdateQuestionValue(
                interviewId: @event.EventSourceId,
                entityId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                value: @event.Event.Answer,
                valueType: NpgsqlDbType.Integer
            );
        }

        public Task Handle(PublishedEvent<NumericRealQuestionAnswered> @event)
        {
            double answer = (double)@event.Event.Answer;
            return UpdateQuestionValue(
                interviewId: @event.EventSourceId,
                entityId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                value: double.IsNaN(answer) ? null : (object)answer,
                valueType: NpgsqlDbType.Double
            );
        }

        public Task Handle(PublishedEvent<TextListQuestionAnswered> @event)
        {
            return UpdateQuestionValue(
                interviewId: @event.EventSourceId,
                entityId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                value: SerializeToJson(@event.Event.Answers),
                valueType: NpgsqlDbType.Json
            );
        }

        public Task Handle(PublishedEvent<MultipleOptionsLinkedQuestionAnswered> @event)
        {
            return UpdateQuestionValue(
                interviewId: @event.EventSourceId,
                entityId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                value: @event.Event.SelectedRosterVectors.Select(c => c.Select(i => (int)i).ToArray()).ToArray(),
                valueType: NpgsqlDbType.Array | NpgsqlDbType.Array | NpgsqlDbType.Integer
            );
        }

        public Task Handle(PublishedEvent<MultipleOptionsQuestionAnswered> @event)
        {
            return UpdateQuestionValue(
                interviewId: @event.EventSourceId,
                entityId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                value: @event.Event.SelectedValues.Select(c => (int)c).ToArray(),
                valueType: NpgsqlDbType.Array | NpgsqlDbType.Integer
            );
        }

        public Task Handle(PublishedEvent<SingleOptionQuestionAnswered> @event)
        {
            return UpdateQuestionValue(
                interviewId: @event.EventSourceId,
                entityId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                value: (int)@event.Event.SelectedValue,
                valueType: NpgsqlDbType.Integer
            );
        }

        public Task Handle(PublishedEvent<SingleOptionLinkedQuestionAnswered> @event)
        {
            return UpdateQuestionValue(
                interviewId: @event.EventSourceId,
                entityId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                value: @event.Event.SelectedRosterVector.Select(c => (int)c).ToArray(),
                valueType: NpgsqlDbType.Array | NpgsqlDbType.Integer
            );
        }

        public Task Handle(PublishedEvent<AreaQuestionAnswered> @event)
        {
            var area = new Area(@event.Event.Geometry, @event.Event.MapName, @event.Event.NumberOfPoints,
                @event.Event.AreaSize, @event.Event.Length, @event.Event.Coordinates, @event.Event.DistanceToEditor);
            return UpdateQuestionValue(
                interviewId: @event.EventSourceId,
                entityId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                value: SerializeToJson(area),
                valueType: NpgsqlDbType.Json
            );
        }

        public Task Handle(PublishedEvent<AudioQuestionAnswered> @event)
        {
            var audioAnswer = AudioAnswer.FromString(@event.Event.FileName, @event.Event.Length);
            return UpdateQuestionValue(
                interviewId: @event.EventSourceId,
                entityId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                value: SerializeToJson(audioAnswer),
                valueType: NpgsqlDbType.Json
            );
        }

        public Task Handle(PublishedEvent<DateTimeQuestionAnswered> @event)
        {
            return UpdateQuestionValue(
                interviewId: @event.EventSourceId,
                entityId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                value: @event.Event.Answer,
                valueType: NpgsqlDbType.Date
            );
        }

        public Task Handle(PublishedEvent<GeoLocationQuestionAnswered> @event)
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
                valueType: NpgsqlDbType.Json
            );
        }

        public Task Handle(PublishedEvent<PictureQuestionAnswered> @event)
        {
            return UpdateQuestionValue(
                interviewId: @event.EventSourceId,
                entityId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                value: @event.Event.PictureFileName,
                valueType: NpgsqlDbType.Text
            );
        }

        public Task Handle(PublishedEvent<QRBarcodeQuestionAnswered> @event)
        {
            return UpdateQuestionValue(
                interviewId: @event.EventSourceId,
                entityId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                value: SerializeToJson(@event.Event.Answer),
                valueType: NpgsqlDbType.Text
            );
        }

        public Task Handle(PublishedEvent<YesNoQuestionAnswered> @event)
        {
            return UpdateQuestionValue(
                interviewId: @event.EventSourceId,
                entityId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                value: SerializeToJson(@event.Event.AnsweredOptions),
                valueType: NpgsqlDbType.Json
            );
        }

        public Task Handle(PublishedEvent<AnswerRemoved> @event)
        {
            return UpdateQuestionValue(
                interviewId: @event.EventSourceId,
                entityId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                value: null,
                valueType: NpgsqlDbType.Unknown
            );
        }

        public async Task Handle(PublishedEvent<AnswersRemoved> @event)
        {
            foreach (var question in @event.Event.Questions)
            {
                await UpdateQuestionValue(
                    interviewId: @event.EventSourceId,
                    entityId: question.Id,
                    rosterVector: question.RosterVector,
                    value: null,
                    valueType: NpgsqlDbType.Unknown
                );
            }
        }

        public async Task Handle(PublishedEvent<QuestionsDisabled> @event)
        {
            foreach (var question in @event.Event.Questions)
            {
                await UpdateEnablementValue(
                    interviewId: @event.EventSourceId,
                    entityId: question.Id,
                    rosterVector: question.RosterVector,
                    isEnabled: false
                );
            }
        }

        public async Task Handle(PublishedEvent<QuestionsEnabled> @event)
        {
            foreach (var question in @event.Event.Questions)
            {
                await UpdateEnablementValue(
                    interviewId: @event.EventSourceId,
                    entityId: question.Id,
                    rosterVector: question.RosterVector,
                    isEnabled: true
                );
            }
        }

        public async Task Handle(PublishedEvent<AnswersDeclaredInvalid> @event)
        {
            var failedValidationConditions = @event.Event.FailedValidationConditions;
            foreach (var question in @event.Event.Questions)
            {
                await UpdateValidityValue(
                    interviewId: @event.EventSourceId,
                    entityId: question.Id,
                    rosterVector: question.RosterVector,
                    validityValue: failedValidationConditions[question].Select(c => c.FailedConditionIndex).ToArray()
                ); 
            }
        }

        public async Task Handle(PublishedEvent<AnswersDeclaredValid> @event)
        {
            foreach (var question in @event.Event.Questions)
            {
                await UpdateValidityValue(
                    interviewId: @event.EventSourceId,
                    entityId: question.Id,
                    rosterVector: question.RosterVector,
                    validityValue: null
                );
            }
        }

        public async Task Handle(PublishedEvent<VariablesChanged> @event)
        {
            foreach (var variable in @event.Event.ChangedVariables)
            {
                var value = variable.NewValue;
                if (value is string sValue && sValue == "NaN")
                    value = double.NaN;
                if (value is double dValue && double.IsNaN(dValue))
                    value = double.NaN;

                await UpdateVariableValue(
                    interviewId: @event.EventSourceId,
                    entityId: variable.Identity.Id,
                    rosterVector: variable.Identity.RosterVector,
                    value: value
                );
            }
        }

        public async Task Handle(PublishedEvent<VariablesDisabled> @event)
        {
            foreach (var identity in @event.Event.Variables)
            {
                await UpdateEnablementValue(
                    interviewId: @event.EventSourceId,
                    entityId: identity.Id,
                    rosterVector: identity.RosterVector,
                    isEnabled: false
                );
            }
        }

        public async Task Handle(PublishedEvent<VariablesEnabled> @event)
        {
            foreach (var identity in @event.Event.Variables)
            {
                await UpdateEnablementValue(
                    interviewId: @event.EventSourceId,
                    entityId: identity.Id,
                    rosterVector: identity.RosterVector,
                    isEnabled: true
                );
            }
        }

        public async Task Handle(PublishedEvent<RosterInstancesAdded> @event)
        {
            foreach (var rosterInstance in @event.Event.Instances)
            {
                var rosterVector = rosterInstance.OuterRosterVector.Append(rosterInstance.RosterInstanceId);
                await AddRoster(@event.EventSourceId, rosterInstance.GroupId, rosterVector);
            }
        }

        public async Task Handle(PublishedEvent<RosterInstancesRemoved> @event)
        {
            foreach (var rosterInstance in @event.Event.Instances)
            {
                var rosterVector = rosterInstance.OuterRosterVector.Append(rosterInstance.RosterInstanceId).ToArray();
                await RemoveRoster(@event.EventSourceId, rosterInstance.GroupId, rosterVector);
            }
        }

        public Task Handle(PublishedEvent<GroupsDisabled> @event)
        {
            /*foreach (var identity in @event.Event.Groups)
            {
                state.Commands.Add(InterviewDataStateChangeCommand.Disable(
                    interviewId: @event.EventSourceId,
                    entityId: identity.Id,
                    rosterVector: identity.RosterVector.Coordinates.ToArray()
                ));
            }*/
            return Task.CompletedTask;
        }

        public Task Handle(PublishedEvent<GroupsEnabled> @event)
        {
            /*foreach (var identity in @event.Event.Groups)
            {
                state.Commands.Add(InterviewDataStateChangeCommand.Enable(
                    interviewId: @event.EventSourceId,
                    entityId: identity.Id,
                    rosterVector: identity.RosterVector.Coordinates.ToArray()
                ));
            }*/
            return Task.CompletedTask;
        }

        private async Task AddInterview(Guid interviewId)
        {
            var questionnaire = await GetQuestionnaireByInterviewIdAsync(interviewId);
            if (questionnaire == null)
                return;

            var topLevelGroups = GetInterviewLevelGroupsWithQuestionOrVariables(questionnaire);
            foreach (var topLevelGroup in topLevelGroups)
            {
                state.InsertInterviewInTable(topLevelGroup.TableName, interviewId);
                state.InsertInterviewInTable(topLevelGroup.EnablementTableName, interviewId);
                state.InsertInterviewInTable(topLevelGroup.ValidityTableName, interviewId);
            }
        }

        private async Task AddRoster(Guid interviewId, Guid groupId, RosterVector rosterVector)
        {
            var questionnaire = await GetQuestionnaireByInterviewIdAsync(interviewId);
            if (questionnaire == null)
                return;

            var @group = questionnaire.Find<Group>(groupId);
            if (@group.Children.Any(e => e is Question || e is Variable))
            {
                state.InsertRosterInTable(@group.TableName, interviewId, rosterVector);
                state.InsertRosterInTable(@group.EnablementTableName, interviewId, rosterVector);
                state.InsertRosterInTable(@group.ValidityTableName, interviewId, rosterVector);
            }
        }

        public async Task UpdateQuestionValue(Guid interviewId, Guid entityId, RosterVector rosterVector, object value, NpgsqlDbType valueType)
        {
            var questionnaire = await GetQuestionnaireByInterviewIdAsync(interviewId);
            if (questionnaire == null)
                return;

            var entity = questionnaire.Find<IQuestionnaireEntity>(entityId);
            var parentGroup = (Group)entity.GetParent();
            var columnName = ((Question)entity).ColumnName;
            state.UpdateValueInTable(parentGroup.TableName, interviewId, rosterVector, columnName, value, valueType);
        }

        public async Task UpdateVariableValue(Guid interviewId, Guid entityId, RosterVector rosterVector, object value)
        {
            var questionnaire = await GetQuestionnaireByInterviewIdAsync(interviewId);
            if (questionnaire == null)
                return;

            var variable = questionnaire.Find<Variable>(entityId);
            var parentGroup = (Group)variable.GetParent();
            var columnName = variable.ColumnName;
            var columnType = GetPostgresSqlTypeForVariable(variable);
            state.UpdateValueInTable(parentGroup.TableName, interviewId, rosterVector, columnName, value, columnType);
        }

        public async Task UpdateEnablementValue(Guid interviewId, Guid entityId, RosterVector rosterVector, bool isEnabled)
        {
            var questionnaire = await GetQuestionnaireByInterviewIdAsync(interviewId);
            if (questionnaire == null)
                return;

            var entity = questionnaire.Find<IQuestionnaireEntity>(entityId);
            var tableName = ResolveGroupForEnablementOrValidity(entity).EnablementTableName;
            var columnName = ResolveColumnNameForEnablementOrValidity(entity);
            state.UpdateValueInTable(tableName, interviewId, rosterVector, columnName, isEnabled, NpgsqlDbType.Boolean);
        }

        public async Task UpdateValidityValue(Guid interviewId, Guid entityId, RosterVector rosterVector, int[] validityValue)
        {
            var questionnaire = await GetQuestionnaireByInterviewIdAsync(interviewId);
            if (questionnaire == null)
                return;
            var entity = questionnaire.Find<IQuestionnaireEntity>(entityId);
            var tableName = ResolveGroupForEnablementOrValidity(entity).ValidityTableName;
            var columnName = ResolveColumnNameForEnablementOrValidity(entity);
            state.UpdateValueInTable(tableName, interviewId, rosterVector, columnName, validityValue, NpgsqlDbType.Array | NpgsqlDbType.Integer);
        }

        public async Task RemoveRoster(Guid interviewId, Guid groupId, RosterVector rosterVector)
        {
            var questionnaire = await GetQuestionnaireByInterviewIdAsync(interviewId);
            if (questionnaire == null)
                return;

            var @group = questionnaire.Find<Group>(groupId);
            if (@group.Children.Any(e => e is Question || e is Variable))
            {
                state.RemoveRosterFromTable(@group.TableName, interviewId, rosterVector);
                state.RemoveRosterFromTable(@group.EnablementTableName, interviewId, rosterVector);
                state.RemoveRosterFromTable(@group.ValidityTableName, interviewId, rosterVector);
            }
        }

        public async Task RemoveInterview(Guid interviewId)
        {
            var questionnaire = await GetQuestionnaireByInterviewIdAsync(interviewId);
            if (questionnaire == null)
                return;

            var topLevelGroups = GetInterviewLevelGroupsWithQuestionOrVariables(questionnaire);
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

            foreach (var tableWithAddInterviews in state.InsertInterviews)
            {
                commands.AddRange(CreateInsertCommandForTable(tableWithAddInterviews.Key, tableWithAddInterviews.Value));
            }

            foreach (var tableWithAddRosters in state.InsertRosters)
            {
                commands.AddRange(CreateAddRosterInstanceForTable(tableWithAddRosters.Key, tableWithAddRosters.Value));
            }

            foreach (var updateValueInfo in state.UpdateValues)
            {
                foreach (var groupedByInterviewAndRoster in updateValueInfo.Value)
                {
                    var updateValueCommand = CreateUpdateValueForTable(updateValueInfo.Key, 
                        groupedByInterviewAndRoster.Key,
                        groupedByInterviewAndRoster.Value);
                    commands.AddRange(updateValueCommand);
                }
            }

            foreach (var tableWithRemoveRosters in state.RemoveRosters)
            {
                commands.AddRange(CreateRemoveRosterInstanceForTable(tableWithRemoveRosters.Key, tableWithRemoveRosters.Value));
            }

            foreach (var tableWithRemoveInterviews in state.RemoveInterviews)
            {
                commands.AddRange(CreateDeleteCommandForTable(tableWithRemoveInterviews.Key, tableWithRemoveInterviews.Value));
            }

            return commands;
        }

        private Task<QuestionnaireDocument> GetQuestionnaireByInterviewIdAsync(Guid interviewId)
        {
            var key = $"{nameof(InterviewDataDenormalizer)}:{tenantContext.Tenant.Name}:{interviewId}";
            return memoryCache.GetOrCreateAsync(key,
                async entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromMinutes(3);

                    var questionnaireId = await this.tenantContext.DbContext.InterviewReferences.FindAsync(interviewId);
                    if (questionnaireId == null)
                        return null;
                    var questionnaire = await questionnaireStorage.GetQuestionnaireAsync(tenantContext.Tenant, new QuestionnaireId(questionnaireId.QuestionnaireId));
                    return questionnaire;
                });
        }

        private IEnumerable<DbCommand> CreateInsertCommandForTable(string tableName, HashSet<Guid> interviewIds)
        {
            foreach (var interviewId in interviewIds)
            {
                var text = $"INSERT INTO \"{tenantContext.Tenant.Name}\".\"{tableName}\" ({InterviewDatabaseConstants.InterviewId})" +
                           $"           VALUES(@interviewId);";
                NpgsqlCommand insertCommand = new NpgsqlCommand(text);
                insertCommand.Parameters.AddWithValue("@interviewId", NpgsqlDbType.Uuid, interviewId);
                yield return insertCommand;
            }
        }

        private IEnumerable<DbCommand> CreateDeleteCommandForTable(string tableName, HashSet<Guid> interviewIds)
        {
            foreach (var interviewId in interviewIds)
            {
                var text = $"DELETE FROM \"{tenantContext.Tenant.Name}\".\"{tableName}\" " +
                           $"      WHERE {InterviewDatabaseConstants.InterviewId} = @interviewId;";
                NpgsqlCommand deleteCommand = new NpgsqlCommand(text);
                deleteCommand.Parameters.AddWithValue("@interviewId", NpgsqlDbType.Uuid, interviewId);
                yield return deleteCommand;
            }
        }

        private IEnumerable<DbCommand> CreateAddRosterInstanceForTable(string tableName, IEnumerable<RosterInfo> rosterInfos)
        {
            foreach (var rosterInfo in rosterInfos)
            {
                var text = $"INSERT INTO \"{tenantContext.Tenant.Name}\".\"{tableName}\" ({InterviewDatabaseConstants.InterviewId}, {InterviewDatabaseConstants.RosterVector})" +
                           $"           VALUES(@interviewId, @rosterVector);";
                NpgsqlCommand insertCommand = new NpgsqlCommand(text);
                insertCommand.Parameters.AddWithValue("@interviewId", NpgsqlDbType.Uuid, rosterInfo.InterviewId);
                insertCommand.Parameters.AddWithValue("@rosterVector", NpgsqlDbType.Array | NpgsqlDbType.Integer, rosterInfo.RosterVector.Coordinates.ToArray());
                yield return insertCommand;
            }
        }

        private IEnumerable<DbCommand> CreateRemoveRosterInstanceForTable(string tableName, IEnumerable<RosterInfo> rosterInfos)
        {
            foreach (var rosterInfo in rosterInfos)
            {
                var text = $"DELETE FROM \"{tenantContext.Tenant.Name}\".\"{tableName}\" " +
                           $"      WHERE {InterviewDatabaseConstants.InterviewId} = @interviewId" +
                           $"        AND {InterviewDatabaseConstants.RosterVector} = @rosterVector;";
                NpgsqlCommand deleteCommand = new NpgsqlCommand(text);
                deleteCommand.Parameters.AddWithValue("@interviewId", NpgsqlDbType.Uuid, rosterInfo.InterviewId);
                deleteCommand.Parameters.AddWithValue("@rosterVector", NpgsqlDbType.Array | NpgsqlDbType.Integer, rosterInfo.RosterVector.Coordinates.ToArray());
                yield return deleteCommand;
            }
        }

        private Group ResolveGroupForEnablementOrValidity(IQuestionnaireEntity entity)
        {
            return (entity as Group) ?? ((Group)entity.GetParent());
        }

        private string ResolveColumnNameForEnablementOrValidity(IQuestionnaireEntity entity)
        {
            switch (entity)
            {
//                case Group group:
//                    return InterviewDatabaseConstants.InstanceValue;
                case Question question:
                    return question.ColumnName;
                case Variable variable:
                    return variable.ColumnName;
                default:
                    throw new ArgumentException("Unsupported entity type: " + entity.GetType().Name);
            }
        }

        private IEnumerable<DbCommand> CreateUpdateValueForTable(string tableName, RosterInfo rosterInfo, IEnumerable<UpdateValueInfo> updateValueInfos)
        {
            bool isTopLevel = rosterInfo.RosterVector == null || rosterInfo.RosterVector.Length == 0;

            foreach (var updateValueInfo in updateValueInfos)
            {
                NpgsqlCommand updateCommand = new NpgsqlCommand();

                var text = $"UPDATE \"{tenantContext.Tenant.Name}\".\"{tableName}\" " +
                           $"   SET {updateValueInfo.ColumnName} = @answer" +
                           $" WHERE {InterviewDatabaseConstants.InterviewId} = @interviewId";
                updateCommand.Parameters.AddWithValue("@interviewId", NpgsqlDbType.Uuid, rosterInfo.InterviewId);

                if (updateValueInfo.Value == null)
                    updateCommand.Parameters.AddWithValue("@answer", DBNull.Value);
                else
                    updateCommand.Parameters.AddWithValue("@answer", updateValueInfo.ValueType, updateValueInfo.Value);

                if (!isTopLevel)
                {
                    text += $"   AND {InterviewDatabaseConstants.RosterVector} = @rosterVector;";
                    updateCommand.Parameters.AddWithValue("@rosterVector", NpgsqlDbType.Array | NpgsqlDbType.Integer, rosterInfo.RosterVector.Coordinates.ToArray());
                }

                updateCommand.CommandText = text;

                yield return updateCommand;
            }
        }

        private NpgsqlDbType GetPostgresSqlTypeForVariable(Variable variable)
        {
            switch (variable.Type)
            {
                case VariableType.Boolean: return NpgsqlDbType.Boolean;
                case VariableType.DateTime: return NpgsqlDbType.Date;
                case VariableType.Double: return NpgsqlDbType.Double;
                case VariableType.LongInteger: return NpgsqlDbType.Bigint;
                case VariableType.String: return NpgsqlDbType.Text;
                default:
                    throw new ArgumentException("Unknown variable type: " + variable.Type);
            }
        }

        private IEnumerable<Group> GetInterviewLevelGroupsWithQuestionOrVariables(QuestionnaireDocument questionnaire)
        {
            var itemsQueue = new Queue<Group>();
            itemsQueue.Enqueue(questionnaire);

            while (itemsQueue.Count > 0)
            {
                var currentGroup = itemsQueue.Dequeue();

                if (currentGroup.Children.Any(e => e is Question || e is Variable))
                    yield return currentGroup;

                var childGroups = currentGroup.Children
                    .Where(g => g is Group childGroup && !childGroup.IsRoster).Cast<Group>();

                foreach (var childItem in childGroups)
                {
                    itemsQueue.Enqueue(childItem);
                }
            }
        }

        private string SerializeToJson(object value)
        {
            return JsonConvert.SerializeObject(value);
        }
    }
}
