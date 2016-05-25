namespace WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog
{
    public enum SynchronizationLogType
    {
        CanSynchronize = 1,
        HasInterviewerDevice,
        LinkToDevice,
        GetInterviewer,
        GetCensusQuestionnaires,
        GetQuestionnaire,
        GetQuestionnaireAssembly,
        QuestionnaireProcessed,
        QuestionnaireAssemblyProcessed,
        GetInterviewPackages,
        GetInterviewPackage,
        InterviewPackageProcessed,
        GetInterviews,
        GetInterview,
        InterviewProcessed,
        GetQuestionnaireAttachments,
        GetAttachmentContent,
        PostInterview,
        PostPackage
    }
}