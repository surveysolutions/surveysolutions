namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    public struct ApplicationVersionSettings
    {
        public int SupportedQuestionnaireVersionMajor { get; set; }

        public int SupportedQuestionnaireVersionMinor { get; set; }

        public int SupportedQuestionnaireVersionPatch { get; set; }
    }
}
