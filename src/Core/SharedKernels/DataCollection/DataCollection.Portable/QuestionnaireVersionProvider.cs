namespace WB.Core.SharedKernels.DataCollection
{
    public class QuestionnaireVersionProvider : IQuestionnaireVersionProvider
    {
        //New Era of c# conditions
        private readonly QuestionnaireVersion version_5 = new QuestionnaireVersion(5, 0, 0);
        private readonly QuestionnaireVersion version_6 = new QuestionnaireVersion(6, 0, 0);
        
        public QuestionnaireVersion GetCurrentEngineVersion()
        {
            return version_6;
        }

        public bool IsClientVersionSupported(QuestionnaireVersion questionnaireVersion, QuestionnaireVersion clientVersion)
        {
            if (questionnaireVersion > clientVersion)
            {
                if (clientVersion < version_5)
                    return false;
            }
            return true;
        }
    }
}
