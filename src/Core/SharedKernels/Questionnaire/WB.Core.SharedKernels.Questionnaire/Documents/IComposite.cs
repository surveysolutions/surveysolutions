using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace Main.Core.Entities.Composite
{
    public interface IComposite : IQuestionnaireEntity
    {
        List<IComposite> Children { get; set; }

        IComposite GetParent();

        void SetParent(IComposite parent);

        T Find<T>(Guid publicKey) where T : class, IComposite;

        IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class;

        T FirstOrDefault<T>(Func<T, bool> condition) where T : class;
        
        void ConnectChildrenWithParent();

        IComposite Clone();
    }
}