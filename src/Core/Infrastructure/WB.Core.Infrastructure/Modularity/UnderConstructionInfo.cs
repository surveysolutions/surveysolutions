using System;
using System.Threading.Tasks;

namespace WB.Core.Infrastructure.Modularity
{
    public class UnderConstructionInfo
    {
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
