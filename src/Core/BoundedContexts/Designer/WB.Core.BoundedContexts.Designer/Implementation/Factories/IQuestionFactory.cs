using Main.Core.Entities;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Implementation.Factories
{
    internal interface IQuestionFactory
    {
        IQuestion CreateQuestion(QuestionData question);
    }
}