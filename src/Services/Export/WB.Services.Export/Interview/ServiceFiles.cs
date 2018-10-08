namespace WB.Services.Export.Interview
{
    public static class ServiceFiles
    {
        public static readonly string Readme = "export__readme";
        public static readonly string InterviewActions = "interview__actions";
        public static readonly string InterviewErrors = "interview__errors";
        public static readonly string InterviewComments = "interview__comments";
        public static readonly string ProtectedVariables = "protected__variables";

        public static readonly string[] AllSystemFiles = {Readme, InterviewActions, InterviewComments, InterviewErrors};
    }
}