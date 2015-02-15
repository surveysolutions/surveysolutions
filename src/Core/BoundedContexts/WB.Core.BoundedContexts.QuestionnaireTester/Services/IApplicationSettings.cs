namespace WB.Core.BoundedContexts.QuestionnaireTester.Services
{
    public interface IApplicationSettings
    {
        string ApplicationVersion { get; }
        string EngineVersion { get; }
        string OSVersion { get; }
        string ApplicationName { get; }
    }
}