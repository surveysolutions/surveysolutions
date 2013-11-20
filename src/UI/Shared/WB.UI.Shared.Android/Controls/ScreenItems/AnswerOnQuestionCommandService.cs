using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.UI.Shared.Android.Controls.ScreenItems
{
    public class AnswerOnQuestionCommandService : IAnswerOnQuestionCommandService
    {
        private readonly ConcurrentDictionary<InterviewItemId, CommandAndErrorCallback> commandQueue =
            new ConcurrentDictionary<InterviewItemId, CommandAndErrorCallback>();

        private readonly Queue<InterviewItemId> executionLine = new Queue<InterviewItemId>();
        private readonly ICommandService commandService;


        public AnswerOnQuestionCommandService(ICommandService commandService)
        {
            this.commandService = commandService;
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    this.ExecuteFirstInLineSaveAnswerCommand();
                    Thread.Sleep(300);
                }
            });
        }

        public void AnswerOnQuestion(AnswerQuestionCommand command, Action<Exception> errorCallback)
        {
            this.UpdateExecutionFlow(new CommandAndErrorCallback(command, errorCallback));
        }

        private void UpdateExecutionFlow(CommandAndErrorCallback command)
        {
            var key = new InterviewItemId(command.Command.QuestionId, command.Command.RosterVector);

            this.commandQueue.AddOrUpdate(key, command, (k, oldValue) => command);

            if (this.executionLine.Count > 0 && this.executionLine.Peek() == key)
                return;

            this.executionLine.Enqueue(key);

        }


        private void ExecuteFirstInLineSaveAnswerCommand()
        {
            CommandAndErrorCallback nextCommand;

            if (this.executionLine.Count == 0)
            {
                return;
            }

            InterviewItemId key = this.executionLine.Dequeue();

            if (!this.commandQueue.TryRemove(key, out nextCommand) || nextCommand == null)
            {
                this.ExecuteFirstInLineSaveAnswerCommand();
                return;
            }

            try
            {
                this.commandService.Execute(nextCommand.Command);
            }
            catch (Exception ex)
            {
                nextCommand.ErrorCallback(ex);
            }
        }

        class CommandAndErrorCallback
        {
            public CommandAndErrorCallback(AnswerQuestionCommand command, Action<Exception> errorCallback)
            {
                this.Command = command;
                this.ErrorCallback = errorCallback;
            }

            public AnswerQuestionCommand Command { get; private set; }
            public Action<Exception> ErrorCallback { get; private set; }
        }
    }
}