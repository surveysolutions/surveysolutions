namespace WB.Core.SharedKernels.DataCollection.Scenarios
{
    public class SwitchTranslation : IScenarioCommand
    {
        public SwitchTranslation(string targetLanguage)
        {
            TargetLanguage = targetLanguage;
        }

        public string TargetLanguage { get; }
    }
}
