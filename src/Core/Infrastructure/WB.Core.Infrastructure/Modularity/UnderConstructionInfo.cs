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
#if DEBUG
            tsc.SetResult(true);
#endif
        }

        public UnderConstructionStatus Status { get; private set; } = UnderConstructionStatus.NotStarted;
        public string Message { get; set; }

#if DEBUG
        private readonly TaskCompletionSource<bool> tsc = new TaskCompletionSource<bool>();
        public Task Completed => tsc.Task;

#endif
        public void ClearMessage()
        {
            Message = null;
        }
    }

    public enum UnderConstructionStatus
    {
        NotStarted = 1,
        Running,
        Finished
    }
}
