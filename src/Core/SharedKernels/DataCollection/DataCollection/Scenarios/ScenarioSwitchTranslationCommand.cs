namespace WB.Core.SharedKernels.DataCollection.Scenarios
{
    public class ScenarioSwitchTranslationCommand : IScenarioCommand
    {
        public ScenarioSwitchTranslationCommand(string targetLanguage)
        {
            TargetLanguage = targetLanguage;
        }

        public string TargetLanguage { get; }
    }
}