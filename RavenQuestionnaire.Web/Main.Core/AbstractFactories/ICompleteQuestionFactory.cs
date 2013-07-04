namespace Main.Core.AbstractFactories
{
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.Events.Questionnaire;

    /// <summary>
    /// The CompleteQuestionFactory interface.
    /// </summary>
    public interface ICompleteQuestionFactory
    {
        ICompleteQuestion ConvertToCompleteQuestion(IQuestion question);

        AbstractQuestion Create(FullQuestionDataEvent type);

        IQuestion CreateQuestionFromExistingUsingDataFromEvent(IQuestion question, QuestionChanged e);
    }
}