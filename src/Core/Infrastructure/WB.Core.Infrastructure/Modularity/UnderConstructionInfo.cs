using System;
using System.Threading.Tasks;

namespace WB.Core.Infrastructure.Modularity
{
    public class UnderConstructionInfo
    {
        private static bool isOneInstanceCreated = false;
        readonly TaskCompletionSource<bool> awaitingBlock = new TaskCompletionSource<bool>();
        
        public UnderConstructionInfo()
        {
            if (isOneInstanceCreated)
                throw new ArgumentException("Allow to create only one instance of UnderConstructionInfo");

            isOneInstanceCreated = true;
        }

        public Task WaitForFinish => awaitingBlock.Task;
        
        public void Run(string message = null)
        {
           Status = UnderConstructionStatus.Running;
           Message = message ?? String.Empty;
        }

        public void Finish()
        {
            Status = UnderConstructionStatus.Finished;
            this.awaitingBlock.SetResult(true);
        }

        public void Error(string message, Exception exception)
        {
            Status = UnderConstructionStatus.Error;
            Message = message;
            Exception = exception;
            
            #if DEBUG
            Message = exception.ToString();
            #endif
            
        }

        public UnderConstructionStatus Status { get; private set; } = UnderConstructionStatus.NotStarted;
        public string Message { get; set; }
        private Exception Exception { get; set; }

        public void ClearMessage()
        {
            if (Status != UnderConstructionStatus.Error)
                Message = null;

        }
    }

    public enum UnderConstructionStatus
    {
        NotStarted = 1,
        Running,
        Finished,
        Error
    }
}
