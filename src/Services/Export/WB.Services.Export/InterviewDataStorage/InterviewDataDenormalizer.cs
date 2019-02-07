using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Npgsql;
using WB.Services.Export.Events.Interview;
using WB.Services.Export.Infrastructure;
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

        public static InterviewDataStateChangeCommand AddRosterInstance(Guid interviewId, Guid groupId, int[] rosterVector)
        {
            return new InterviewDataStateChangeCommand()
            {
                Type = InterviewDataStateChangeCommandType.AddRosterInstance,
                InterviewId = interviewId,
                RosterVector = rosterVector
            };
        }

        public static InterviewDataStateChangeCommand RemoveRosterInstance(Guid interviewId, int[] rosterVector)
        {
            return new InterviewDataStateChangeCommand()
            {
                Type = InterviewDataStateChangeCommandType.RemoveRosterInstance,
                InterviewId = interviewId,
                RosterVector = rosterVector
            };
        }

        public static InterviewDataStateChangeCommand UpdateAnswer(Guid interviewId, Guid questionId, int[] rosterVector,
            string answer)
        {
            return new InterviewDataStateChangeCommand()
            {
                Type = InterviewDataStateChangeCommandType.UpdateAnswer,
                InterviewId = interviewId,
                EntityId = questionId,
                RosterVector = rosterVector,
                TableType = InterviewDataStateChangeTableType.Data,
                ColumnValue = answer
            };
        }

        public InterviewDataStateChangeTableType TableType { get; private set; }
        public Guid InterviewId { get; private set; }
        public InterviewDataStateChangeCommandType Type { get; private set; }
        public Guid EntityId { get; private set; }
        public int[] RosterVector { get; private set; }
        public string ColumnValue { get; private set; }
    }

    public enum InterviewDataStateChangeCommandType
    {
        InsertInterview = 1,
        UpdateAnswer,
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

    class InterviewDataDenormalizer:
        IFunctionalHandler,
        IEventHandler<InterviewCreated>,
        IEventHandler<RosterInstancesAdded>,
        IEventHandler<TextQuestionAnswered>/*,
        IEventHandler<NumericIntegerQuestionAnswered>*/
    {
        private readonly ITenantContext tenantContext;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IInterviewQuestionnaireReferenceStorage interviewQuestionnaireReference;
        
        private readonly InterviewDataState state;

        public InterviewDataDenormalizer(ITenantContext tenantContext, IQuestionnaireStorage questionnaireStorage,
            IInterviewQuestionnaireReferenceStorage interviewQuestionnaireReference)
        {
            this.tenantContext = tenantContext;
            this.questionnaireStorage = questionnaireStorage;
            this.interviewQuestionnaireReference = interviewQuestionnaireReference;

            state = new InterviewDataState()
            {
                Commands = new List<InterviewDataStateChangeCommand>()
            };
        }

        public Task HandleAsync(PublishedEvent<InterviewCreated> @event, CancellationToken cancellationToken)
        {
            state.Commands.Add(InterviewDataStateChangeCommand.InsertInterview(@event.EventSourceId));
            return Task.FromResult(state);
        }

        public Task HandleAsync(PublishedEvent<RosterInstancesAdded> @event, CancellationToken cancellationToken)
        {
            foreach (var rosterInstance in @event.Event.Instances)
            {
                var rosterVector = rosterInstance.OuterRosterVector.Append(rosterInstance.RosterInstanceId)
                    .Cast<int>().ToArray();
                state.Commands.Add(InterviewDataStateChangeCommand.AddRosterInstance(
                    @event.EventSourceId, rosterInstance.GroupId, rosterVector));
            }
            return Task.FromResult(state);
        }

        public Task HandleAsync(PublishedEvent<TextQuestionAnswered> @event, CancellationToken cancellationToken)
        {
            state.Commands.Add(InterviewDataStateChangeCommand.UpdateAnswer(
                interviewId: @event.EventSourceId,
                questionId: @event.Event.QuestionId,
                rosterVector: @event.Event.RosterVector.Cast<int>().ToArray(),
                answer: @event.Event.Answer
            ));
            return Task.FromResult(state);
        }

//        public Task<InterviewDataState> Handle(InterviewDataState state, PublishedEvent<NumericIntegerQuestionAnswered> @event, CancellationToken cancellationToken)
//        {
//            state.Commands.Add(new InterviewDataStateChangeCommand()
//            {
//                InterviewId = @event.EventSourceId,
//                Type = InterviewDataStateChangeCommandType.UpdateAnswer,
//                TableType = InterviewDataStateChangeTableType.Data,
//                EntityId = @event.Event.QuestionId,
//                RosterVector = @event.Event.RosterVector,
//                ColumnValue = @event.Event.Answer
//            });
//            return Task.FromResult(state);
//        }


        private class CommandsForExecute
        {
            public List<SqlCommand> InsertInterviewCommands { get; set; } = new List<SqlCommand>();
            public List<SqlCommand> AddRosterInstanceCommands { get; set; } = new List<SqlCommand>();
            public List<SqlCommand> UpdateAnswerCommands { get; set; } = new List<SqlCommand>();
            public List<SqlCommand> RemoveRosterInstanceCommands { get; set; } = new List<SqlCommand>();
            public List<SqlCommand> RemoveInterviewCommands { get; set; } = new List<SqlCommand>();

            public IEnumerable<SqlCommand> GetOrderedCommands()
            {
                return InsertInterviewCommands
                    .Concat(AddRosterInstanceCommands)
                    .Concat(UpdateAnswerCommands)
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
                sqlCommand.Connection = tenantContext.Connection;
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
                var questionnaire = await GetQuestionnaireByInterviewIdAsync(interviewId, cancellationToken);

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
                        case InterviewDataStateChangeCommandType.UpdateAnswer:
                        {
                            foreach (var updateAnswerCommand in commandByType)
                                commandsState.UpdateAnswerCommands.AddRange(GetUpdateAnswerCommands(questionnaire, updateAnswerCommand));
                            break;
                        }
                        default:
                            //throw new ArgumentException("Unknown command type: " + commandByType.Key);
                            break; // ignore
                    }
                }
            }

            return commandsState;
        }

        private async Task<QuestionnaireDocument> GetQuestionnaireByInterviewIdAsync(Guid interviewId, CancellationToken cancellationToken)
        {
            var questionnaireId = await interviewQuestionnaireReference.GetQuestionnaireIdByInterviewIdAsync(interviewId, cancellationToken);
            var questionnaire = await questionnaireStorage.GetQuestionnaireAsync(tenantContext.Tenant, questionnaireId);
            return questionnaire;
        }

        private IEnumerable<SqlCommand> GetInsertCommands(QuestionnaireDocument questionnaire, InterviewDataStateChangeCommand commandInfo)
        {
            var topLevelGroups = GetInterviewLevelGroupsWithQuestionOrVariables(questionnaire);

            foreach (var topLevelGroup in topLevelGroups)
            {
                yield return CreateInsertCommandForTable(topLevelGroup.TableName, commandInfo.InterviewId);
                yield return CreateInsertCommandForTable(topLevelGroup.EnablementTableName, commandInfo.InterviewId);
                yield return CreateInsertCommandForTable(topLevelGroup.ValidityTableName, commandInfo.InterviewId);
            }
        }

        private SqlCommand CreateInsertCommandForTable(string tableName, Guid interviewId)
        {
            var text = $"INSERT INTO \"{tenantContext.Tenant.Name}\".\"{tableName}\" ({InterviewDatabaseConstants.InterviewId})" +
                       $"           VALUES(@interviewId);";
            SqlCommand insertCommand = new SqlCommand(text);
            insertCommand.Parameters.AddWithValue("@interviewId", interviewId);
            return insertCommand;
        }

        private IEnumerable<SqlCommand> GetDeleteCommands(QuestionnaireDocument questionnaire, InterviewDataStateChangeCommand commandInfo)
        {
            var topLevelGroups = GetInterviewLevelGroupsWithQuestionOrVariables(questionnaire);

            foreach (var topLevelGroup in topLevelGroups)
            {
                yield return CreateDeleteCommandForTable(topLevelGroup.TableName, commandInfo.InterviewId);
                yield return CreateDeleteCommandForTable(topLevelGroup.EnablementTableName, commandInfo.InterviewId);
                yield return CreateDeleteCommandForTable(topLevelGroup.ValidityTableName, commandInfo.InterviewId);
            }
        }

        private SqlCommand CreateDeleteCommandForTable(string tableName, Guid interviewId)
        {
            var text = $"DELETE FROM \"{tenantContext.Tenant.Name}\".\"{tableName}\" " +
                       $"      WHERE {InterviewDatabaseConstants.InterviewId} = @interviewId;";
            SqlCommand deleteCommand = new SqlCommand(text);
            deleteCommand.Parameters.AddWithValue("@interviewId", interviewId);
            return deleteCommand;
        }

        private IEnumerable<SqlCommand> GetAddRosterInstanceCommands(QuestionnaireDocument questionnaire, InterviewDataStateChangeCommand commandInfo)
        {
            var @group = questionnaire.Find<Group>(commandInfo.EntityId);
            yield return CreateAddRosterInstanceForTable(@group.TableName, commandInfo.InterviewId, commandInfo.RosterVector);
            yield return CreateAddRosterInstanceForTable(@group.EnablementTableName, commandInfo.InterviewId, commandInfo.RosterVector);
            yield return CreateAddRosterInstanceForTable(@group.ValidityTableName, commandInfo.InterviewId, commandInfo.RosterVector);
        }

        private SqlCommand CreateAddRosterInstanceForTable(string tableName, Guid interviewId, int[] rosterVector)
        {
            var text = $"INSERT INTO \"{tenantContext.Tenant.Name}\".\"{tableName}\" ({InterviewDatabaseConstants.InterviewId}, {InterviewDatabaseConstants.RosterVector})" +
                       $"           VALUES(@interviewId, @rosterVector);";
            SqlCommand insertCommand = new SqlCommand(text);
            insertCommand.Parameters.AddWithValue("@interviewId", interviewId);
            insertCommand.Parameters.AddWithValue("@rosterVector", rosterVector);
            return insertCommand;
        }

        private IEnumerable<SqlCommand> GetRemoveRosterInstanceCommands(QuestionnaireDocument questionnaire, InterviewDataStateChangeCommand commandInfo)
        {
            var @group = questionnaire.Find<Group>(commandInfo.EntityId);
            yield return CreateRemoveRosterInstanceForTable(@group.TableName, commandInfo.InterviewId, commandInfo.RosterVector);
            yield return CreateRemoveRosterInstanceForTable(@group.EnablementTableName, commandInfo.InterviewId, commandInfo.RosterVector);
            yield return CreateRemoveRosterInstanceForTable(@group.ValidityTableName, commandInfo.InterviewId, commandInfo.RosterVector);
        }

        private SqlCommand CreateRemoveRosterInstanceForTable(string tableName, Guid interviewId, int[] rosterVector)
        {
            var text = $"DELETE FROM \"{tenantContext.Tenant.Name}\".\"{tableName}\" " +
                       $"      WHERE {InterviewDatabaseConstants.InterviewId} = @interviewId" +
                       $"        AND {InterviewDatabaseConstants.RosterVector} = @rosterVector;";
            SqlCommand deleteCommand = new SqlCommand(text);
            deleteCommand.Parameters.AddWithValue("@interviewId", interviewId);
            deleteCommand.Parameters.AddWithValue("@rosterVector", rosterVector);
            return deleteCommand;
        }

        private IEnumerable<SqlCommand> GetUpdateAnswerCommands(QuestionnaireDocument questionnaire, InterviewDataStateChangeCommand commandInfo)
        {
            var question = questionnaire.Find<Question>(commandInfo.EntityId);
            var parentGroup = (Group) question.GetParent();
            yield return CreateUpdateAnswerForTable(parentGroup.TableName, question.ColumnName, commandInfo.InterviewId, commandInfo.RosterVector, commandInfo.ColumnValue);
        }

        private SqlCommand CreateUpdateAnswerForTable(string tableName, string columnName, Guid interviewId, int[] rosterVector, string answer)
        {
            var text = $"UPDATE \"{tenantContext.Tenant.Name}\".\"{tableName}\" " +
                       $"   SET {columnName} = @answer" +
                       $" WHERE {InterviewDatabaseConstants.InterviewId} = @interviewId" +
                       $"   AND {InterviewDatabaseConstants.RosterVector} = @rosterVector;";
            SqlCommand updateCommand = new SqlCommand(text);
            updateCommand.Parameters.AddWithValue("@answer", answer);
            updateCommand.Parameters.AddWithValue("@interviewId", interviewId);
            updateCommand.Parameters.AddWithValue("@rosterVector", rosterVector);
            return updateCommand;
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
    }
}
