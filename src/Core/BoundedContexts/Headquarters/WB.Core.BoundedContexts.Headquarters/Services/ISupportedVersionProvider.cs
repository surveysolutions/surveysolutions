namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface ISupportedVersionProvider
    {
        int GetSupportedQuestionnaireVersion();
        int? GetMinVerstionSupportedByInterviewer();
        void RememberMinSupportedVersion();
    }
}
