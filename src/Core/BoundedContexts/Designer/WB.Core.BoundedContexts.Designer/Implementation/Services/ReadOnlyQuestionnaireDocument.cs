using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
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
        public QuestionnaireDocument Questionnaire { get; private set; }
        public string Translation { get; private set; }

        public ReadOnlyQuestionnaireDocument(QuestionnaireDocument questionnaire, string translation = null)
        {
            this.Translation = translation;
            this.Questionnaire = questionnaire;
            this.Questionnaire.ConnectChildrenWithParent();
            this.allItems = CreateEntitiesIdAndTypePairsInQuestionnaireFlowOrder(this.Questionnaire);
        }

        public Dictionary<Guid, Macro> Macros => this.Questionnaire.Macros;
        public Dictionary<Guid, LookupTable> LookupTables => this.Questionnaire.LookupTables;
        public List<Attachment> Attachments => this.Questionnaire.Attachments;
        public List<Translation> Translations => this.Questionnaire.Translations;
        public string Title => this.Questionnaire.Title;
        public Guid PublicKey => this.Questionnaire.PublicKey;

        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            return this.allItems.FirstOrDefault(x => x is T && x.PublicKey==publicKey) as T;
        }

        public IEnumerable<T> Find<T>() where T : class
            => this.allItems.Where(x => x is T).Cast<T>();

        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class
            => this.Find<T>().Where(condition);

        public T FirstOrDefault<T>(Func<T, bool> condition) where T : class
            => this.Find(condition).FirstOrDefault();

        public IEnumerable<QuestionnaireItemTypeReference> GetAllEntitiesIdAndTypePairsInQuestionnaireFlowOrder()
        {
            return this.allItems.Select(x => new QuestionnaireItemTypeReference(x.PublicKey, x.GetType()));
        }

        private IComposite[] CreateEntitiesIdAndTypePairsInQuestionnaireFlowOrder(QuestionnaireDocument questionnaire)
        {
            var result = new List<IComposite>();
            var stack = new Stack<IComposite>();
            stack.Push(questionnaire);
            while (stack.Any())
            {
                var current = stack.Pop();
                for (int i = current.Children.Count - 1; i >= 0; i--)
                {
                    var child = current.Children[i];
                    stack.Push(child);
                }
                result.Add(current);
            }
            return result.Skip(1).ToArray();
        }

        public IEnumerable<T> FindInGroup<T>(Guid groupId)
        {
            var group = Find<IGroup>(groupId);
            return group.TreeToEnumerableDepthFirst<IComposite>(x => x.Children).Where(x => x.PublicKey!=groupId && x is T).Cast<T>();
        }
    }
}