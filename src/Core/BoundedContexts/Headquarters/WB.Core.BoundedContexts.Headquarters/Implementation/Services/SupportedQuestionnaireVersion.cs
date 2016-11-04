namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    public class SupportedQuestionnaireVersion
    {
        public virtual string Id { get; set; }
        public virtual int MinQuestionnaireVersionSupportedByInterviewer { get; set; }
    }
}