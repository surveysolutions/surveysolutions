namespace WB.Core.SharedKernels.DataCollection
{
    public interface IQuestionnaireVersionProvider
    {
        QuestionnaireVersion GetCurrentEngineVersion();

        bool IsClientVersionSupported(QuestionnaireVersion questionnaireVersion, QuestionnaireVersion clientVersion);
    }
}