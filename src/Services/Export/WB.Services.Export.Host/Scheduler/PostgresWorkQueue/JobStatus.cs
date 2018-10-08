namespace WB.Services.Export.Host.Scheduler.PostgresWorkQueue
{
    public enum JobStatus
    {
        Created = 0,
        Running = 1,
        Completed = 2,
        Fail = 3,
        Canceled = 4
    }
}
