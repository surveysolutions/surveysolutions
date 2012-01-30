using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.Entities.Composite
{
    public interface IComposite
    {
        void Add(IComposite c, Guid? parent);
        void Remove(IComposite c);
        void Remove<T>(Guid publicKey) where T : class, IComposite;
        T Find<T>(Guid publicKey) where T : class, IComposite;
    }
}
