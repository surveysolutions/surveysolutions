using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using Main.Core.Commands.Questionnaire.Completed;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
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
            UpdateExecutionFlow(new CommandAndErrorCallback(command, errorCallback));
        }

        private void UpdateExecutionFlow(CommandAndErrorCallback command)
        {
            var key = new InterviewItemId(command.Command.QuestionId, command.Command.PropagationVector);

            commandQueue.AddOrUpdate(key, command, (k, oldValue) => command);

            if (executionLine.Count > 0 && executionLine.Peek() == key)
                return;

            executionLine.Enqueue(key);

        }


        private void ExecuteFirstInLineSaveAnswerCommand()
        {
            CommandAndErrorCallback nextCommand;

            if (executionLine.Count == 0)
            {
                return;
            }

            InterviewItemId key = executionLine.Dequeue();

            if (!commandQueue.TryRemove(key, out nextCommand) || nextCommand == null)
            {
                this.ExecuteFirstInLineSaveAnswerCommand();
                return;
            }

            try
            {
                commandService.Execute(nextCommand.Command);
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
                Command = command;
                ErrorCallback = errorCallback;
            }

            public AnswerQuestionCommand Command { get; private set; }
            public Action<Exception> ErrorCallback { get; private set; }
        }
    }
}