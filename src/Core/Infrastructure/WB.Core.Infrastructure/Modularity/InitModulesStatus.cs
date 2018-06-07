namespace WB.Core.Infrastructure.Modularity
{
    public class InitModulesStatus
    {
        public ServerInitializingStatus Status { get; set; } = ServerInitializingStatus.NotStarted;
        public string Message { get; set; }
    }

    public enum ServerInitializingStatus
    {
        NotStarted = 1,
        Running,
        Finished
    }
}
