using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete.Question
{
    public interface IAnswerStrategy
    {
        void Add(IComposite c, Guid? parent);
        void Remove();
        void Create(IEnumerable<IComposite> answers);
    }
}