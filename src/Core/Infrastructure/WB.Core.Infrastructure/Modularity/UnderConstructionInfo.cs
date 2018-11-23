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

        public void Error(string message)
        {
            Status = UnderConstructionStatus.Error;
        }

        public UnderConstructionStatus Status { get; private set; } = UnderConstructionStatus.NotStarted;
        public string Message { get; set; }

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
