namespace WB.Core.SharedKernels.DataCollection
{
    public static class QuestionnaireVersionProvider
    {
        //New Era of c# conditions
        private static readonly QuestionnaireVersion version_5 = new QuestionnaireVersion(5, 0, 0);
        
        public static QuestionnaireVersion GetCurrentEngineVersion()
        {
            return version_5;
        }
    }
}
