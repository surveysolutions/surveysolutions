namespace Main.Core.AbstractFactories
{
    using Main.Core.Entities;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;

    public interface ICompleteQuestionFactory
    {
        ICompleteQuestion ConvertToCompleteQuestion(IQuestion question);
        AbstractQuestion CreateQuestion(DataQuestion question);
    }
}