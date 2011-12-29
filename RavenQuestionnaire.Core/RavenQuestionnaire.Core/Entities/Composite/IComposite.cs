using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.Entities.Composite
{
    public interface IComposite
    {
        bool Add(IComposite c, Guid? parent);
        bool Remove(IComposite c);
        bool Remove<T>(Guid publicKey) where T : class, IComposite;
        T Find<T>(Guid publicKey) where T : class, IComposite;
    }
}
