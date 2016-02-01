using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    public class ReadOnlyQuestionnaireDocument
    {
        public class QuestionnaireItemTypeReference
        {
            public Guid Id { get; }
            public Type Type { get; }

            public QuestionnaireItemTypeReference(Guid id, Type type)
            {
                this.Id = id;
                this.Type = type;
            }
        }
        private readonly IEnumerable<IComposite> allItems;
        private QuestionnaireDocument questionnaire { get; set; }

        public ReadOnlyQuestionnaireDocument(QuestionnaireDocument questionnaire)
        {
            this.questionnaire = questionnaire;
            this.questionnaire.ConnectChildrenWithParent();
            this.allItems = this.questionnaire.Children.SelectMany<IComposite, IComposite>(x => x.TreeToEnumerable<IComposite>(g => g.Children)).ToList();
        }

        public Dictionary<Guid, Macro> Macros => this.questionnaire.Macros;
        public Dictionary<Guid, LookupTable> LookupTables => this.questionnaire.LookupTables;
        public string Title => this.questionnaire.Title;
        public Guid PublicKey => this.questionnaire.PublicKey;

        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            return this.allItems.FirstOrDefault(x => x is T && x.PublicKey==publicKey) as T;
        }

        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class
        {
            return this.allItems.Where(x => x is T && condition((T) x)).Cast<T>();
        }

        public T FirstOrDefault<T>(Func<T, bool> condition) where T : class
        {
            return this.allItems.Where(a => a is T && condition((T) a)).Cast<T>().FirstOrDefault();
        }

        public IEnumerable<QuestionnaireItemTypeReference> GetAllEntitiesIdAndTypePairs()
        {
            return this.allItems.Select(x => new QuestionnaireItemTypeReference(x.PublicKey, x.GetType()));
        }
    }
}