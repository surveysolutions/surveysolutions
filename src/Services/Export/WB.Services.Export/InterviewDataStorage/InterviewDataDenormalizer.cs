using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using WB.Services.Export.Events;
using WB.Services.Export.Events.Interview;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Questionnaire.Services;
using WB.Services.Infrastructure.EventSourcing;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.InterviewDataStorage
{
    public class InterviewDataState
    {
        public List<InterviewDataStateChangeCommand> Commands { get; set; }
    }

    public class InterviewDataStateChangeCommand
    {
        private InterviewDataStateChangeCommand()
        {
        }

        public static InterviewDataStateChangeCommand InsertInterview(Guid interviewId)
        {
            return new InterviewDataStateChangeCommand()
            {
                Type = InterviewDataStateChangeCommandType.InsertInterview,
                InterviewId = interviewId
            };
        }

        public static InterviewDataStateChangeCommand RemoveInterview(Guid interviewId)
        {
            return new InterviewDataStateChangeCommand()
            {
                Type = InterviewDataStateChangeCommandType.DeleteInterview,
                InterviewId = interviewId
            };
        }

        public static InterviewDataStateChangeCommand AddRosterInstance(Guid interviewId, Guid groupId, RosterVector rosterVector)
        {
            return new InterviewDataStateChangeCommand()
            {
                Type = InterviewDataStateChangeCommandType.AddRosterInstance,
                InterviewId = interviewId,
                EntityId = groupId,
                RosterVector = rosterVector.Select(i => (int)i).ToArray()
            };
        }

        public static InterviewDataStateChangeCommand RemoveRosterInstance(Guid interviewId, Guid groupId, RosterVector rosterVector)
        {
            return new InterviewDataStateChangeCommand()
            {
                Type = InterviewDataStateChangeCommandType.RemoveRosterInstance,
                InterviewId = interviewId,
                EntityId = groupId,
                RosterVector = rosterVector.Select(i => (int)i).ToArray()
            };
        }

        public static InterviewDataStateChangeCommand UpdateAnswer(Guid interviewId, Guid questionId, RosterVector rosterVector,
            object answer, NpgsqlDbType answerType)
        {
            return new InterviewDataStateChangeCommand()
            {
                Type = InterviewDataStateChangeCommandType.UpdateValue,
                InterviewId = interviewId,
                EntityId = questionId,
                RosterVector = rosterVector.Select(i => (int)i).ToArray(),
                TableType = InterviewDataStateChangeTableType.Data,
                Answer = answer,
                AnswerType = answerType
            };
        }

        public static InterviewDataStateChangeCommand RemoveAnswer(Guid interviewId, Guid questionId, int[] rosterVector)
        {
            return new InterviewDataStateChangeCommand()
            {
                Type = InterviewDataStateChangeCommandType.UpdateValue,
                InterviewId = interviewId,
                EntityId = questionId,
                RosterVector = rosterVector,
                TableType = InterviewDataStateChangeTableType.Data,
                Answer = null
            };
        }

        public static InterviewDataStateChangeCommand UpdateVariable(Guid interviewId, Guid variableId, int[] rosterVector,
            object value)
        {
            return new InterviewDataStateChangeCommand()
            {
                Type = InterviewDataStateChangeCommandType.UpdateValue,
                InterviewId = interviewId,
                EntityId = variableId,
                RosterVector = rosterVector,
                TableType = InterviewDataStateChangeTableType.Data,
                Answer = value,
            };
        }

        public static InterviewDataStateChangeCommand Enable(Guid interviewId, Guid entityId, int[] rosterVector)
        {
            return new InterviewDataStateChangeCommand()
            {
                Type = InterviewDataStateChangeCommandType.UpdateValue,
                InterviewId = interviewId,
                EntityId = entityId,
                RosterVector = rosterVector.Select(i => (int)i).ToArray(),
                TableType = InterviewDataStateChangeTableType.Enablement,
                Answer = true,
                AnswerType = NpgsqlDbType.Boolean
            };
        }

        public static InterviewDataStateChangeCommand Disable(Guid interviewId, Guid entityId, int[] rosterVector)
        {
            return new InterviewDataStateChangeCommand()
            {
                Type = InterviewDataStateChangeCommandType.UpdateValue,
                InterviewId = interviewId,
                EntityId = entityId,
                RosterVector = rosterVector.Select(i => (int)i).ToArray(),
                TableType = InterviewDataStateChangeTableType.Enablement,
                Answer = false,
                AnswerType = NpgsqlDbType.Boolean
            };
        }

        public static InterviewDataStateChangeCommand Invalid(Guid interviewId, Guid entityId, int[] rosterVector,
            int[] failedValidationConditions)
        {
            return new InterviewDataStateChangeCommand()
            {
                Type = InterviewDataStateChangeCommandType.UpdateValue,
                InterviewId = interviewId,
                EntityId = entityId,
                RosterVector = rosterVector.Select(i => (int)i).ToArray(),
                TableType = InterviewDataStateChangeTableType.Validity,
                Answer = failedValidationConditions,
                AnswerType = NpgsqlDbType.Array | NpgsqlDbType.Integer
            };
        }

        public static InterviewDataStateChangeCommand Valid(Guid interviewId, Guid entityId, int[] rosterVector)
        {
            return new InterviewDataStateChangeCommand()
            {
                Type = InterviewDataStateChangeCommandType.UpdateValue,
                InterviewId = interviewId,
                EntityId = entityId,
                RosterVector = rosterVector.Select(i => (int)i).ToArray(),
                TableType = InterviewDataStateChangeTableType.Validity,
                Answer = null,
                AnswerType = NpgsqlDbType.Array | NpgsqlDbType.Integer
            };
        }
        public InterviewDataStateChangeTableType TableType { get; private set; }
        public Guid InterviewId { get; private set; }
        public InterviewDataStateChangeCommandType Type { get; private set; }
        public Guid EntityId { get; private set; }
        public RosterVector RosterVector { get; private set; }
        public NpgsqlDbType AnswerType { get; private set; }
        public object Answer { get; private set; }

    }

    public enum InterviewDataStateChangeCommandType
    {
        InsertInterview = 1,
        UpdateValue,
        DeleteInterview,
        AddRosterInstance,
        RemoveRosterInstance,
    }

    public enum InterviewDataStateChangeTableType
    {
        Data = 1,
        Validity,
        Enablement
    }

    public class InterviewDataDenormalizer :
       // IFunctionalHandler,
        IEventHandler<InterviewCreated>,
        IEventHandler<InterviewFromPreloadedDataCreated>,
        IEventHandler<InterviewOnClientCreated>,
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

        private readonly InterviewDataState state;

        public InterviewDataDenormalizer(ITenantContext tenantContext, IQuestionnaireStorage questionnaireStorage)
        {
            this.tenantContext = tenantContext;
            this.questionnaireStorage = questionnaireStorage;

            state = new InterviewDataState()
            {
                Commands = new List<InterviewDataStateChangeCommand>()
            };
        }

        public void Handle(PublishedEvent<InterviewCreated> @event)
        {
            state.Commands.Add(InterviewDataStateChangeCommand.InsertInterview(@event.EventSourceId));
        }

        public void Handle(PublishedEvent<InterviewFromPreloadedDataCreated> @event)
        {
            state.Commands.Add(InterviewDataStateChangeCommand.InsertInterview(@event.EventSourceId));
        }

        public void Handle(PublishedEvent<InterviewOnClientCreated> @event)
        {
            //state.Commands.Add(InterviewDataStateChangeCommand.InsertInterview(@event.EventSourceId));

        }

        public void Handle(PublishedEvent<InterviewHardDeleted> @event)
        {
            state.Commands.Add(InterviewDataStateChangeCommand.RemoveInterview(@event.EventSourceId));
        }

        public void Handle(PublishedEvent<TextQuestionAnswered> @event)
        {
            state.Commands.Add(InterviewDataStateChangeCommand.UpdateAnswer(
                interviewId: @event.EventSourceId,
                questionId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                answer: @event.Event.Answer,
                answerType: NpgsqlDbType.Text
            ));
        }

        public void Handle(PublishedEvent<NumericIntegerQuestionAnswered> @event)
        {
            state.Commands.Add(InterviewDataStateChangeCommand.UpdateAnswer(
                interviewId: @event.EventSourceId,
                questionId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                answer: @event.Event.Answer,
                answerType: NpgsqlDbType.Integer
            ));
        }

        public void Handle(PublishedEvent<NumericRealQuestionAnswered> @event)
        {
            double answer = (double)@event.Event.Answer;
            state.Commands.Add(InterviewDataStateChangeCommand.UpdateAnswer(
                interviewId: @event.EventSourceId,
                questionId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                answer: double.IsNaN(answer) ? null : (object)answer,
                answerType: NpgsqlDbType.Double
            ));
        }

        public void Handle(PublishedEvent<TextListQuestionAnswered> @event)
        {
            state.Commands.Add(InterviewDataStateChangeCommand.UpdateAnswer(
                interviewId: @event.EventSourceId,
                questionId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                answer: SerializeToJson(@event.Event.Answers),
                answerType: NpgsqlDbType.Json
            ));
        }

        public void Handle(PublishedEvent<MultipleOptionsLinkedQuestionAnswered> @event)
        {
            state.Commands.Add(InterviewDataStateChangeCommand.UpdateAnswer(
                interviewId: @event.EventSourceId,
                questionId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                answer: @event.Event.SelectedRosterVectors.Select(c => c.Select(i => (int)i).ToArray()).ToArray(),
                answerType: NpgsqlDbType.Array | NpgsqlDbType.Array | NpgsqlDbType.Integer
            ));
        }

        public void Handle(PublishedEvent<MultipleOptionsQuestionAnswered> @event)
        {
            state.Commands.Add(InterviewDataStateChangeCommand.UpdateAnswer(
                interviewId: @event.EventSourceId,
                questionId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                answer: @event.Event.SelectedValues.Select(c => (int)c).ToArray(),
                answerType: NpgsqlDbType.Array | NpgsqlDbType.Integer
            ));
        }

        public void Handle(PublishedEvent<SingleOptionQuestionAnswered> @event)
        {
            state.Commands.Add(InterviewDataStateChangeCommand.UpdateAnswer(
                interviewId: @event.EventSourceId,
                questionId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                answer: (int)@event.Event.SelectedValue,
                answerType: NpgsqlDbType.Integer
            ));
        }

        public void Handle(PublishedEvent<SingleOptionLinkedQuestionAnswered> @event)
        {
            state.Commands.Add(InterviewDataStateChangeCommand.UpdateAnswer(
                interviewId: @event.EventSourceId,
                questionId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                answer: @event.Event.SelectedRosterVector.Select(c => (int)c).ToArray(),
                answerType: NpgsqlDbType.Array | NpgsqlDbType.Integer
            ));
        }

        public void Handle(PublishedEvent<AreaQuestionAnswered> @event)
        {
            var area = new Area(@event.Event.Geometry, @event.Event.MapName, @event.Event.NumberOfPoints,
                @event.Event.AreaSize, @event.Event.Length, @event.Event.Coordinates, @event.Event.DistanceToEditor);
            state.Commands.Add(InterviewDataStateChangeCommand.UpdateAnswer(
                interviewId: @event.EventSourceId,
                questionId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                answer: SerializeToJson(area),
                answerType: NpgsqlDbType.Json
            ));
        }

        public void Handle(PublishedEvent<AudioQuestionAnswered> @event)
        {
            var audioAnswer = AudioAnswer.FromString(@event.Event.FileName, @event.Event.Length);
            state.Commands.Add(InterviewDataStateChangeCommand.UpdateAnswer(
                interviewId: @event.EventSourceId,
                questionId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                answer: SerializeToJson(audioAnswer),
                answerType: NpgsqlDbType.Json
            ));
        }

        public void Handle(PublishedEvent<DateTimeQuestionAnswered> @event)
        {
            state.Commands.Add(InterviewDataStateChangeCommand.UpdateAnswer(
                interviewId: @event.EventSourceId,
                questionId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                answer: @event.Event.Answer,
                answerType: NpgsqlDbType.Date
            ));
        }

        public void Handle(PublishedEvent<GeoLocationQuestionAnswered> @event)
        {
            GeoPosition geoPosition = new GeoPosition(@event.Event.Latitude,
                @event.Event.Longitude,
                @event.Event.Accuracy,
                @event.Event.Altitude,
                @event.Event.Timestamp);
            state.Commands.Add(InterviewDataStateChangeCommand.UpdateAnswer(
                interviewId: @event.EventSourceId,
                questionId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                answer: SerializeToJson(geoPosition),
                answerType: NpgsqlDbType.Json
            ));
        }

        public void Handle(PublishedEvent<PictureQuestionAnswered> @event)
        {
            state.Commands.Add(InterviewDataStateChangeCommand.UpdateAnswer(
                interviewId: @event.EventSourceId,
                questionId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                answer: @event.Event.PictureFileName,
                answerType: NpgsqlDbType.Text
            ));
        }

        public void Handle(PublishedEvent<QRBarcodeQuestionAnswered> @event)
        {
            state.Commands.Add(InterviewDataStateChangeCommand.UpdateAnswer(
                interviewId: @event.EventSourceId,
                questionId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                answer: SerializeToJson(@event.Event.Answer),
                answerType: NpgsqlDbType.Text
            ));
        }

        public void Handle(PublishedEvent<YesNoQuestionAnswered> @event)
        {
            state.Commands.Add(InterviewDataStateChangeCommand.UpdateAnswer(
                interviewId: @event.EventSourceId,
                questionId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector,
                answer: SerializeToJson(@event.Event.AnsweredOptions),
                answerType: NpgsqlDbType.Json
            ));
        }

        public void Handle(PublishedEvent<AnswerRemoved> @event)
        {
            state.Commands.Add(InterviewDataStateChangeCommand.RemoveAnswer(
                interviewId: @event.EventSourceId,
                questionId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector.Select(i => (int)i).ToArray()
            ));
        }

        public void Handle(PublishedEvent<AnswersRemoved> @event)
        {
            foreach (var question in @event.Event.Questions)
            {
                state.Commands.Add(InterviewDataStateChangeCommand.RemoveAnswer(
                    interviewId: @event.EventSourceId,
                    questionId: question.Id,
                    rosterVector: question.RosterVector.Coordinates.ToArray()
                ));
            }
        }

        public void Handle(PublishedEvent<QuestionsDisabled> @event)
        {
            foreach (var question in @event.Event.Questions)
            {
                state.Commands.Add(InterviewDataStateChangeCommand.Disable(
                    interviewId: @event.EventSourceId,
                    entityId: question.Id,
                    rosterVector: question.RosterVector.Coordinates.ToArray()
                ));
            }
        }

        public void Handle(PublishedEvent<QuestionsEnabled> @event)
        {
            foreach (var question in @event.Event.Questions)
            {
                state.Commands.Add(InterviewDataStateChangeCommand.Enable(
                    interviewId: @event.EventSourceId,
                    entityId: question.Id,
                    rosterVector: question.RosterVector.Coordinates.ToArray()
                ));
            }
        }

        public void Handle(PublishedEvent<AnswersDeclaredInvalid> @event)
        {
            var failedValidationConditions = @event.Event.FailedValidationConditions;
            foreach (var question in @event.Event.Questions)
            {
                state.Commands.Add(InterviewDataStateChangeCommand.Invalid(
                    interviewId: @event.EventSourceId,
                    entityId: question.Id,
                    rosterVector: question.RosterVector.Coordinates.ToArray(),
                    failedValidationConditions: failedValidationConditions[question].Select(c => c.FailedConditionIndex).ToArray()
                ));
            }
        }

        public void Handle(PublishedEvent<AnswersDeclaredValid> @event)
        {
            foreach (var question in @event.Event.Questions)
            {
                state.Commands.Add(InterviewDataStateChangeCommand.Valid(
                    interviewId: @event.EventSourceId,
                    entityId: question.Id,
                    rosterVector: question.RosterVector.Coordinates.ToArray()
                ));
            }
        }

        public void Handle(PublishedEvent<VariablesChanged> @event)
        {
            foreach (var variable in @event.Event.ChangedVariables)
            {
                var value = variable.NewValue;
                if (value is string sValue && sValue == "NaN")
                    value = double.NaN;
                if (value is double dValue && double.IsNaN(dValue))
                    value = double.NaN;

                state.Commands.Add(InterviewDataStateChangeCommand.UpdateVariable(
                    interviewId: @event.EventSourceId,
                    variableId: variable.Identity.Id,
                    rosterVector: variable.Identity.RosterVector.Coordinates.ToArray(),
                    value: value
                ));
            }
        }

        public void Handle(PublishedEvent<VariablesDisabled> @event)
        {
            foreach (var identity in @event.Event.Variables)
            {
                state.Commands.Add(InterviewDataStateChangeCommand.Disable(
                    interviewId: @event.EventSourceId,
                    entityId: identity.Id,
                    rosterVector: identity.RosterVector.Coordinates.ToArray()
                ));
            }
        }

        public void Handle(PublishedEvent<VariablesEnabled> @event)
        {
            foreach (var identity in @event.Event.Variables)
            {
                state.Commands.Add(InterviewDataStateChangeCommand.Enable(
                    interviewId: @event.EventSourceId,
                    entityId: identity.Id,
                    rosterVector: identity.RosterVector.Coordinates.ToArray()
                ));
            }
        }

        public void Handle(PublishedEvent<RosterInstancesAdded> @event)
        {
            foreach (var rosterInstance in @event.Event.Instances)
            {
                var rosterVector = rosterInstance.OuterRosterVector.Append(rosterInstance.RosterInstanceId);
                state.Commands.Add(InterviewDataStateChangeCommand.AddRosterInstance(
                    @event.EventSourceId,
                    rosterInstance.GroupId,
                    rosterVector));
            }
        }

        public void Handle(PublishedEvent<RosterInstancesRemoved> @event)
        {
            foreach (var rosterInstance in @event.Event.Instances)
            {
                var rosterVector = rosterInstance.OuterRosterVector.Append(rosterInstance.RosterInstanceId).ToArray();
                state.Commands.Add(InterviewDataStateChangeCommand.RemoveRosterInstance(@event.EventSourceId, rosterInstance.GroupId, rosterVector));
            }
        }

        public void Handle(PublishedEvent<GroupsDisabled> @event)
        {
            /*foreach (var identity in @event.Event.Groups)
            {
                state.Commands.Add(InterviewDataStateChangeCommand.Disable(
                    interviewId: @event.EventSourceId,
                    entityId: identity.Id,
                    rosterVector: identity.RosterVector.Coordinates.ToArray()
                ));
            }*/
        }

        public void Handle(PublishedEvent<GroupsEnabled> @event)
        {
            /*foreach (var identity in @event.Event.Groups)
            {
                state.Commands.Add(InterviewDataStateChangeCommand.Enable(
                    interviewId: @event.EventSourceId,
                    entityId: identity.Id,
                    rosterVector: identity.RosterVector.Coordinates.ToArray()
                ));
            }*/
        }

        private class CommandsForExecute
        {
            public List<DbCommand> InsertInterviewCommands { get; set; } = new List<DbCommand>();
            public List<DbCommand> AddRosterInstanceCommands { get; set; } = new List<DbCommand>();
            public List<DbCommand> UpdateValueCommands { get; set; } = new List<DbCommand>();
            public List<DbCommand> RemoveRosterInstanceCommands { get; set; } = new List<DbCommand>();
            public List<DbCommand> RemoveInterviewCommands { get; set; } = new List<DbCommand>();

            public IEnumerable<DbCommand> GetOrderedCommands()
            {
                return InsertInterviewCommands
                    .Concat(AddRosterInstanceCommands)
                    .Concat(UpdateValueCommands)
                    .Concat(RemoveRosterInstanceCommands)
                    .Concat(RemoveInterviewCommands);
            }
        }

        public async Task SaveStateAsync(CancellationToken cancellationToken)
        {
            var commandsState = await GenerateSqlCommandsAsync(cancellationToken);
            await ExecuteCommandsAsync(commandsState.GetOrderedCommands(), cancellationToken);
        }

        private async Task ExecuteCommandsAsync(IEnumerable<DbCommand> sqlCommands, CancellationToken cancellationToken)
        {


            foreach (var sqlCommand in sqlCommands)
            {
                sqlCommand.Connection = tenantContext.DbContext.Database.GetDbConnection();
                await sqlCommand.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        private async Task<CommandsForExecute> GenerateSqlCommandsAsync(CancellationToken cancellationToken)
        {
            var commandsState = new CommandsForExecute();

            var commands = state.Commands;
            var commandsByInterviews = commands.GroupBy(c => c.InterviewId);

            foreach (var commandsByInterview in commandsByInterviews)
            {
                var interviewId = commandsByInterview.Key;
                var questionnaire = await GetQuestionnaireByInterviewIdAsync(interviewId);
                if (questionnaire == null)
                    continue;

                var commandsByType = commandsByInterview.GroupBy(c => c.Type);
                foreach (var commandByType in commandsByType)
                {
                    switch (commandByType.Key)
                    {
                        case InterviewDataStateChangeCommandType.InsertInterview:
                            {
                                foreach (var insertInterviewCommand in commandByType)
                                    commandsState.InsertInterviewCommands.AddRange(GetInsertCommands(questionnaire, insertInterviewCommand));
                                break;
                            }
                        case InterviewDataStateChangeCommandType.DeleteInterview:
                            {
                                foreach (var deleteInterviewCommand in commandByType)
                                    commandsState.RemoveInterviewCommands.AddRange(GetDeleteCommands(questionnaire, deleteInterviewCommand));
                                break;
                            }
                        case InterviewDataStateChangeCommandType.AddRosterInstance:
                            {
                                foreach (var addRosterInstanceCommand in commandByType)
                                    commandsState.AddRosterInstanceCommands.AddRange(GetAddRosterInstanceCommands(questionnaire, addRosterInstanceCommand));
                                break;
                            }
                        case InterviewDataStateChangeCommandType.RemoveRosterInstance:
                            {
                                foreach (var removeRosterInstanceCommand in commandByType)
                                    commandsState.RemoveRosterInstanceCommands.AddRange(GetRemoveRosterInstanceCommands(questionnaire, removeRosterInstanceCommand));
                                break;
                            }
                        case InterviewDataStateChangeCommandType.UpdateValue:
                            {
                                foreach (var updateAnswerCommand in commandByType)
                                    commandsState.UpdateValueCommands.AddRange(GetUpdateValueCommands(questionnaire, updateAnswerCommand));
                                break;
                            }
                        default:
                            throw new ArgumentException("Unknown command type: " + commandByType.Key);
                            //break; // ignore
                    }
                }
            }

            return commandsState;
        }

        private async Task<QuestionnaireDocument> GetQuestionnaireByInterviewIdAsync(Guid interviewId)
        {
            var questionnaireId = await this.tenantContext.DbContext.InterviewReferences.FindAsync(interviewId);
            if (questionnaireId == null)
                return null;
            var questionnaire = await questionnaireStorage.GetQuestionnaireAsync(tenantContext.Tenant, new QuestionnaireId(questionnaireId.QuestionnaireId));
            return questionnaire;
        }

        private IEnumerable<DbCommand> GetInsertCommands(QuestionnaireDocument questionnaire, InterviewDataStateChangeCommand commandInfo)
        {
            var topLevelGroups = GetInterviewLevelGroupsWithQuestionOrVariables(questionnaire);

            foreach (var topLevelGroup in topLevelGroups)
            {
                yield return CreateInsertCommandForTable(topLevelGroup.TableName, commandInfo.InterviewId);
                yield return CreateInsertCommandForTable(topLevelGroup.EnablementTableName, commandInfo.InterviewId);
                yield return CreateInsertCommandForTable(topLevelGroup.ValidityTableName, commandInfo.InterviewId);
            }
        }

        private DbCommand CreateInsertCommandForTable(string tableName, Guid interviewId)
        {
            var text = $"INSERT INTO \"{tenantContext.Tenant.Name}\".\"{tableName}\" ({InterviewDatabaseConstants.InterviewId})" +
                       $"           VALUES(@interviewId);";
            NpgsqlCommand insertCommand = new NpgsqlCommand(text);
            insertCommand.Parameters.AddWithValue("@interviewId", NpgsqlDbType.Uuid, interviewId);
            return insertCommand;
        }

        private IEnumerable<DbCommand> GetDeleteCommands(QuestionnaireDocument questionnaire, InterviewDataStateChangeCommand commandInfo)
        {
            var topLevelGroups = GetInterviewLevelGroupsWithQuestionOrVariables(questionnaire);

            foreach (var topLevelGroup in topLevelGroups)
            {
                yield return CreateDeleteCommandForTable(topLevelGroup.TableName, commandInfo.InterviewId);
                yield return CreateDeleteCommandForTable(topLevelGroup.EnablementTableName, commandInfo.InterviewId);
                yield return CreateDeleteCommandForTable(topLevelGroup.ValidityTableName, commandInfo.InterviewId);
            }
        }

        private DbCommand CreateDeleteCommandForTable(string tableName, Guid interviewId)
        {
            var text = $"DELETE FROM \"{tenantContext.Tenant.Name}\".\"{tableName}\" " +
                       $"      WHERE {InterviewDatabaseConstants.InterviewId} = @interviewId;";
            NpgsqlCommand deleteCommand = new NpgsqlCommand(text);
            deleteCommand.Parameters.AddWithValue("@interviewId", NpgsqlDbType.Uuid, interviewId);
            return deleteCommand;
        }

        private IEnumerable<DbCommand> GetAddRosterInstanceCommands(QuestionnaireDocument questionnaire, InterviewDataStateChangeCommand commandInfo)
        {
            var @group = questionnaire.Find<Group>(commandInfo.EntityId);
            if (@group.Children.Any(e => e is Question || e is Variable))
            {
                yield return CreateAddRosterInstanceForTable(@group.TableName, commandInfo.InterviewId, commandInfo.RosterVector);
                yield return CreateAddRosterInstanceForTable(@group.EnablementTableName, commandInfo.InterviewId, commandInfo.RosterVector);
                yield return CreateAddRosterInstanceForTable(@group.ValidityTableName, commandInfo.InterviewId, commandInfo.RosterVector);
            }
        }

        private DbCommand CreateAddRosterInstanceForTable(string tableName, Guid interviewId, RosterVector rosterVector)
        {
            var text = $"INSERT INTO \"{tenantContext.Tenant.Name}\".\"{tableName}\" ({InterviewDatabaseConstants.InterviewId}, {InterviewDatabaseConstants.RosterVector})" +
                       $"           VALUES(@interviewId, @rosterVector);";
            NpgsqlCommand insertCommand = new NpgsqlCommand(text);
            insertCommand.Parameters.AddWithValue("@interviewId", NpgsqlDbType.Uuid, interviewId);
            insertCommand.Parameters.AddWithValue("@rosterVector", NpgsqlDbType.Array | NpgsqlDbType.Integer, rosterVector);
            return insertCommand;
        }

        private IEnumerable<DbCommand> GetRemoveRosterInstanceCommands(QuestionnaireDocument questionnaire, InterviewDataStateChangeCommand commandInfo)
        {
            var @group = questionnaire.Find<Group>(commandInfo.EntityId);
            yield return CreateRemoveRosterInstanceForTable(@group.TableName, commandInfo.InterviewId, commandInfo.RosterVector);
            yield return CreateRemoveRosterInstanceForTable(@group.EnablementTableName, commandInfo.InterviewId, commandInfo.RosterVector);
            yield return CreateRemoveRosterInstanceForTable(@group.ValidityTableName, commandInfo.InterviewId, commandInfo.RosterVector);
        }

        private DbCommand CreateRemoveRosterInstanceForTable(string tableName, Guid interviewId, RosterVector rosterVector)
        {
            var text = $"DELETE FROM \"{tenantContext.Tenant.Name}\".\"{tableName}\" " +
                       $"      WHERE {InterviewDatabaseConstants.InterviewId} = @interviewId" +
                       $"        AND {InterviewDatabaseConstants.RosterVector} = @rosterVector;";
            NpgsqlCommand deleteCommand = new NpgsqlCommand(text);
            deleteCommand.Parameters.AddWithValue("@interviewId", NpgsqlDbType.Uuid, interviewId);
            deleteCommand.Parameters.AddWithValue("@rosterVector", NpgsqlDbType.Array | NpgsqlDbType.Integer, rosterVector);
            return deleteCommand;
        }

        private IEnumerable<DbCommand> GetUpdateValueCommands(QuestionnaireDocument questionnaire, InterviewDataStateChangeCommand commandInfo)
        {
            var entity = questionnaire.Find<IQuestionnaireEntity>(commandInfo.EntityId);

            switch (commandInfo.TableType)
            {
                case InterviewDataStateChangeTableType.Data:
                    {
                        var parentGroup = (Group)entity.GetParent();
                        var columnName = (entity as Question)?.ColumnName ?? ((Variable)entity).ColumnName;
                        var columnType = GetPostgresSqlTypeForEntity(entity, commandInfo);
                        yield return CreateUpdateValueForTable(parentGroup.TableName, columnName, commandInfo.InterviewId, commandInfo.RosterVector, commandInfo.Answer, columnType);
                    }
                    break;
                case InterviewDataStateChangeTableType.Enablement:
                    {
                        var tableName = ResolveGroupForEnablementOrValidity(entity).EnablementTableName;
                        var columnName = ResolveColumnNameForEnablementOrValidity(entity);
                        yield return CreateUpdateValueForTable(tableName, columnName, commandInfo.InterviewId, commandInfo.RosterVector, commandInfo.Answer, NpgsqlDbType.Boolean);
                    }
                    break;
                case InterviewDataStateChangeTableType.Validity:
                    {
                        var tableName = ResolveGroupForEnablementOrValidity(entity).ValidityTableName;
                        var columnName = ResolveColumnNameForEnablementOrValidity(entity);
                        yield return CreateUpdateValueForTable(tableName, columnName, commandInfo.InterviewId, commandInfo.RosterVector, commandInfo.Answer, NpgsqlDbType.Array | NpgsqlDbType.Integer);
                    }
                    break;
                default:
                    throw new ArgumentException("Unknown table type");
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

        private DbCommand CreateUpdateValueForTable(string tableName, string columnName, Guid interviewId, RosterVector rosterVector, object answer, NpgsqlDbType answerType)
        {
            bool isTopLevel = rosterVector == null || rosterVector.Length == 0;

            NpgsqlCommand updateCommand = new NpgsqlCommand();

            var text = $"UPDATE \"{tenantContext.Tenant.Name}\".\"{tableName}\" " +
                       $"   SET {columnName} = @answer" +
                       $" WHERE {InterviewDatabaseConstants.InterviewId} = @interviewId";
            updateCommand.Parameters.AddWithValue("@interviewId", NpgsqlDbType.Uuid, interviewId);

            if (answer == null)
                updateCommand.Parameters.AddWithValue("@answer", DBNull.Value);
            else
                updateCommand.Parameters.AddWithValue("@answer", answerType, answer);

            if (!isTopLevel)
            {
                text += $"   AND {InterviewDatabaseConstants.RosterVector} = @rosterVector;";
                updateCommand.Parameters.AddWithValue("@rosterVector", NpgsqlDbType.Array | NpgsqlDbType.Integer, rosterVector);
            }

            updateCommand.CommandText = text;

            return updateCommand;
        }

        private NpgsqlDbType GetPostgresSqlTypeForEntity(IQuestionnaireEntity entity, InterviewDataStateChangeCommand commandInfo)
        {
            if (entity is Question)
                return commandInfo.AnswerType;

            var variable = (Variable)entity;
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

                var childGroups = currentGroup.Children.Where(g => g is Group childGroup && !childGroup.IsRoster).Cast<Group>();

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
