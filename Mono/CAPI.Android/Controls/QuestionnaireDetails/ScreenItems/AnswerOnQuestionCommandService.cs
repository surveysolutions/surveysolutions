using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
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
        private readonly Dictionary<InterviewItemId, CommandAndErrorCallback> commandQueue =
            new Dictionary<InterviewItemId, CommandAndErrorCallback>();

        private readonly Queue<InterviewItemId> executionLine = new Queue<InterviewItemId>();
        private readonly ICommandService commandService;
        private readonly object locker = new object();
        private bool isRunning;

        public AnswerOnQuestionCommandService(ICommandService commandService)
        {
            this.commandService = commandService;
        }

        public void AnswerOnQuestion(Context context, AnswerQuestionCommand command, Action<Exception> errorCallback)
        {
            UpdateExecutionFlow(new CommandAndErrorCallback(command, errorCallback));
           /* Toast.MakeText(context,
                           string.Format("in line: {0}, commandQueue: {1}", executionLine.Count, commandQueue.Count),
                           ToastLength.Short).Show();*/
            if (!isRunning)
            {
                isRunning = true;
                Task.Factory.StartNew(ExecuteSaveAnswerCommandAndRunNextIfExist);
            }
        }

        private void UpdateExecutionFlow(CommandAndErrorCallback command)
        {
            var key = new InterviewItemId(command.Command.QuestionId, command.Command.PropagationVector);

            lock (locker)
            {
                commandQueue[key] = command;

                if (executionLine.Count > 0 && executionLine.Peek() == key)
                    return;

                executionLine.Enqueue(key);
            }
        }


        private void ExecuteSaveAnswerCommandAndRunNextIfExist()
        {
            CommandAndErrorCallback nextCommand = null;

            lock (locker)
            {
                if (executionLine.Count == 0)
                {
                    isRunning = false;
                    return;
                }

                InterviewItemId key = executionLine.Dequeue();

                if (commandQueue.ContainsKey(key))
                {
                    nextCommand = commandQueue[key];
                    commandQueue.Remove(key);
                }
            }

            try
            {
                if (nextCommand != null)
                    commandService.Execute(nextCommand.Command);
            }
            catch (Exception ex)
            {
                if (nextCommand != null)
                    nextCommand.ErrorCallback(ex);
            }

            ExecuteSaveAnswerCommandAndRunNextIfExist();
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