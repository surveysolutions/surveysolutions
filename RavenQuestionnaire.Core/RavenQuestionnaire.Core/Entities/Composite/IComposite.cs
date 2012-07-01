using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.Entities.Composite
{
    public interface IComposite 
    {
        Guid PublicKey { get; }
        void Add(IComposite c, Guid? parent);
        void Remove(IComposite c);
        void Remove(Guid publicKey);
        T Find<T>(Guid publicKey) where T : class, IComposite;
        IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class;
        T FirstOrDefault<T>(Func<T, bool> condition) where T : class;


        List<IComposite> Children { get; set; }
        
        IComposite Parent { get;}
    }

    
}
