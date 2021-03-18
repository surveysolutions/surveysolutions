namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class SqliteSettings
    {
        public string PathToDatabaseDirectory { get; set; }
        public string InterviewsDirectory { get; set; }
        public bool InMemoryStorage { get; set; } = false;
    }
}