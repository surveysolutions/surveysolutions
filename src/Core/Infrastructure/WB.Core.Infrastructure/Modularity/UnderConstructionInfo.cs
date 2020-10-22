using System;

namespace WB.Core.Infrastructure.Modularity
{
    public class UnderConstructionInfo
    {
        private static bool isOneInstanceCreated = false;
        
        public UnderConstructionInfo()
        {
            if (isOneInstanceCreated)
                throw new ArgumentException("Allow to create only one instance of UnderConstructionInfo");

            isOneInstanceCreated = true;
        }

        public void Run()
        {
           Status = UnderConstructionStatus.Running;
        }

        public void Finish()
        {
            Status = UnderConstructionStatus.Finished;
        }

        public void Error(string message, Exception exception)
        {
            Status = UnderConstructionStatus.Error;
            Message = message;
            Exception = exception;
            //AwaitingBlock.SetException(exception);
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
