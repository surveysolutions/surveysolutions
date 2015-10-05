namespace WB.UI.Interviewer.Syncronization.Implementation
{
    public class InterviewPackageIdsStorageSettings
    {
        public InterviewPackageIdsStorageSettings(string pathToDatabase, string databaseName)
        {
            this.PathToDatabase = pathToDatabase;
            this.DatabaseName = databaseName;
        }

        public string PathToDatabase { get; private set; }
        public string DatabaseName { get; private set; }
    }
}