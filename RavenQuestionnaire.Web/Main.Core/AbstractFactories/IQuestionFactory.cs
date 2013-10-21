using Main.Core.Entities;
using Main.Core.Entities.SubEntities;

namespace Main.Core.AbstractFactories
{
    public interface IQuestionFactory {
        AbstractQuestion CreateQuestion(QuestionData question);
    }
}