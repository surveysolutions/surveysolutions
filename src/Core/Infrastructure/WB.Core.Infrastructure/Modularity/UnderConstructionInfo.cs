namespace WB.Core.Infrastructure.Modularity
{
    public class UnderConstructionInfo
    {
        public UnderConstructionStatus Status { get; set; } = UnderConstructionStatus.NotStarted;
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
        Finished
    }
}
