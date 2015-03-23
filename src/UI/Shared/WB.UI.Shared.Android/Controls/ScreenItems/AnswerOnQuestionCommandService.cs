using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
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
        private readonly IAnswerProgressIndicator answerProgressIndicator;
        private static readonly object LockObject = new object();


        public AnswerOnQuestionCommandService(ICommandService commandService, IAnswerProgressIndicator answerProgressIndicator)
        {
            this.commandService = commandService;
            this.answerProgressIndicator = answerProgressIndicator;

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    this.ExecuteFirstInLineSaveAnswerCommand();

                    Thread.Sleep(100);
                }
            });
        }

        public void AnswerOnQuestion(AnswerQuestionCommand command, Action<Exception> errorCallback, Action<string> succeedCallback = null)
        {
            this.UpdateExecutionFlow(new CommandAndErrorCallback(command, errorCallback, succeedCallback));
        }

        public void ExecuteAllCommandsInRow()
        {
            while (this.executionLine.Count > 0)
            {
                this.ExecuteFirstInLineSaveAnswerCommand();
            }
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
            lock (LockObject)
            {
                if (this.executionLine.Count == 0)
                    return;

                this.answerProgressIndicator.Show();
                try
                {
                    this.ExecuteFirstInLineSaveAnswerCommandImpl();
                }
                finally
                {
                    this.answerProgressIndicator.Hide();
                }
            }
        }

        private void ExecuteFirstInLineSaveAnswerCommandImpl()
        {
            CommandAndErrorCallback nextCommand;

            InterviewItemId key = this.executionLine.Dequeue();

            if (!this.commandQueue.TryRemove(key, out nextCommand) || nextCommand == null)
            {
                this.ExecuteFirstInLineSaveAnswerCommand();
                return;
            }

            try
            {
                this.commandService.Execute(nextCommand.Command);
                nextCommand.SucceedCallback("Ok");
            }
            catch (Exception ex)
            {
                nextCommand.ErrorCallback(ex);
            }
        }

        class CommandAndErrorCallback
        {
            public CommandAndErrorCallback(AnswerQuestionCommand command, Action<Exception> errorCallback, Action<string> succeedCallback = null)
            {
                this.Command = command;
                this.ErrorCallback = errorCallback;
                this.SucceedCallback = succeedCallback;
            }

            public AnswerQuestionCommand Command { get; private set; }
            public Action<Exception> ErrorCallback { get; private set; }
            public Action<string> SucceedCallback { get; private set; }
        }
    }
}