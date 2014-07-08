using System;
using Main.Core.Entities;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Implementation.Factories
{
    internal interface IQuestionnaireEntityFactory
    {
        IStaticText CreateStaticText(Guid entityId, string text);
        IQuestion CreateQuestion(QuestionData question);
    }
}