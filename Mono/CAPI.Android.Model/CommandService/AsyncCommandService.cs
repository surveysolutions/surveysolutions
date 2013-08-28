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
using Ncqrs.Commanding.ServiceModel;

namespace CAPI.Android.Core.Model.CommandService
{
    public class AsyncCommandService : Ncqrs.Commanding.ServiceModel.CommandService
    {
        private readonly ConcurrentQueue<Action> runningTasks = new ConcurrentQueue<Action>();
        private bool isRunning = false;

        public override void Execute(Ncqrs.Commanding.ICommand command)
        {
            var newTask = new Action(() => ExecuteCommandWithTryCatch(command));
            runningTasks.Enqueue(newTask);

            if (!isRunning)
            {
                isRunning = true;
                Task.Factory.StartNew(RunNextInLine);
            }
        }

        public void ExecuteSynchronously(Ncqrs.Commanding.ICommand command)
        {
            base.Execute(command);

        }

        protected void ExecuteCommandWithTryCatch(Ncqrs.Commanding.ICommand command)
        {
            try
            {
                base.Execute(command);
                RunNextInLine();
            }
            catch (Exception e)
            {
                LogExcetion(e);
            }
        }

        private void LogExcetion(Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
            if (e.InnerException != null)
                LogExcetion(e.InnerException);
        }

        protected void RunNextInLine()
        {
            Action currentTask;

            runningTasks.TryDequeue(out currentTask);

            if (currentTask == null)
            {
                isRunning = false;
                return;
            }

            currentTask();

        }
    }
}
