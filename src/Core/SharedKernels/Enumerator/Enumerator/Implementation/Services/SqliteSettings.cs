namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class SqliteSettings
    {
        public string PathToRootDirectory { get; set; }
        public string PathToDatabaseDirectory { get; set; }
        public string DataDirectoryName { get; set; }
        public string InterviewsDirectoryName { get; set; }
        public bool InMemoryStorage { get; set; } = false;
    }
}