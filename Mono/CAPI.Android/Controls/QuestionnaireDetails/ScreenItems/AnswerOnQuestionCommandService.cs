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

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public class AnswerOnQuestionCommandService : IAnswerOnQuestionCommandService
    {
        private readonly Dictionary<ItemPublicKey, AnswerQuestionCommand> commandQueue =
            new Dictionary<ItemPublicKey, AnswerQuestionCommand>();

        private readonly Queue<ItemPublicKey> executionLine = new Queue<ItemPublicKey>();
        private readonly ICommandService commandService;
        private readonly object locker = new object();
        private bool isRunning;

        public AnswerOnQuestionCommandService(ICommandService commandService)
        {
            this.commandService = commandService;
        }

        public void Execute(AnswerQuestionCommand command)
        {
            UpdateExecutionFlow(command);

            if (!isRunning)
            {
                isRunning = true;
                Task.Factory.StartNew(ExecuteSaveAnswerCommandAndRunNextIfExist);
            }
        }

        private void UpdateExecutionFlow(AnswerQuestionCommand command)
        {
            var key = new ItemPublicKey(command.QuestionId, command.PropagationVector);

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
            AnswerQuestionCommand nextCommand = null;

            lock (locker)
            {
                if (executionLine.Count == 0)
                {
                    isRunning = false;
                    return;
                }

                ItemPublicKey key = executionLine.Dequeue();
                nextCommand = commandQueue[key];
                commandQueue.Remove(key);
            }

            try
            {
                if (nextCommand != null)
                    commandService.Execute(nextCommand);
            }
            catch (Exception ex)
            {
                /*((Activity)this.Context).RunOnUiThread(() =>
                {
                    tvError.Visibility = ViewStates.Visible;
                    tvError.Text = GetDippestException(ex).Message;
                    SaveAnswerErrorHappend();
                });*/
            }

            ExecuteSaveAnswerCommandAndRunNextIfExist();
        }
    }
}