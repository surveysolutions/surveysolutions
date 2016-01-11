using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    public class CachedQuestionnaireDocument
    {
        private readonly IEnumerable<IComposite> allItems;

        public CachedQuestionnaireDocument(QuestionnaireDocument questionnaire)
        {
            this.Questionnaire = questionnaire;
            this.Questionnaire.ConnectChildrenWithParent();
            this.allItems = this.Questionnaire.Children.SelectMany<IComposite, IComposite>(x => x.TreeToEnumerable<IComposite>(g => g.Children)).ToList();
        }

        public QuestionnaireDocument Questionnaire { get; private set; }
        public Dictionary<Guid, Macro> Macros => this.Questionnaire.Macros;
        public Dictionary<Guid, LookupTable> LookupTables => this.Questionnaire.LookupTables;
        public string Title => this.Questionnaire.Title;
        public Guid PublicKey => this.Questionnaire.PublicKey;

        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            return this.allItems.FirstOrDefault(x => x is T && x.PublicKey==publicKey) as T;
        }

        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class
        {
            return this.allItems.Where(x => x is T && condition((T) x)).Select(a => a as T);
        }

        public T FirstOrDefault<T>(Func<T, bool> condition) where T : class
        {
            return this.allItems.Where(a => a is T && condition((T) a)).Select(a => a as T).FirstOrDefault();
        }

        public IEnumerable<Tuple<Guid, Type>> GetAllEntitiesIds()
        {
            return this.allItems.Select(x => new Tuple<Guid, Type>(x.PublicKey, x.GetType()));
        }
    }
}