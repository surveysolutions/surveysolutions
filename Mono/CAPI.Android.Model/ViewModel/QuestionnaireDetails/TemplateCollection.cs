using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails
{
    public class TemplateCollection : IEnumerable<QuestionnairePropagatedScreenViewModel>
    {
        private readonly Dictionary<Guid, QuestionnairePropagatedScreenViewModel> collection =
            new Dictionary<Guid, QuestionnairePropagatedScreenViewModel>();

        private readonly Dictionary<Guid, IList<Guid>> scopes =
          new Dictionary<Guid, IList<Guid>>();
        public void Add(Guid key, QuestionnairePropagatedScreenViewModel value)
        {
            collection.Add(key, value);
            if (!scopes.Where(s => s.Value.Contains(key)).Select(s => s.Key).Any())
                scopes.Add(key, new List<Guid> {key});
        }

        public QuestionnairePropagatedScreenViewModel this[Guid key]
        {
            get { return collection[key]; }
        }
        public void AssignScope(Guid scopeId, IEnumerable<Guid> keys)
        {
            foreach (var guid in keys)
            {
                var scopeBy = scopes.Where(s => s.Value.Contains(guid)).Select(s => s.Key);
                if (!scopeBy.Any())
                    continue;
                var currentScope = scopeBy.First();
                var itemsInCurrentScope = scopes[currentScope];
                if (itemsInCurrentScope.Count == 1)
                {
                    scopes.Remove(currentScope);
                }
                else
                {
                    itemsInCurrentScope.Remove(guid);
                }
            }
            scopes.Add(scopeId, keys.ToList());
        }

        public IEnumerable<Guid> GetItemsInScope(Guid scopeKey)
        {
            if (!scopes.ContainsKey(scopeKey))
                throw new ArgumentException("item is absent in any scope");
            return scopes[scopeKey];
        }

        public IEnumerable<Guid> GetScopeByItem(Guid scopedItem)
        {
            var itemScopes = GetItemScope(scopedItem);
            return GetItemsInScope(itemScopes);
        }
        public Guid GetItemScope(Guid itemKey)
        {
            var itemScopes = scopes.Where(s => s.Value.Contains(itemKey)).Select(s => s.Key);
            if (!itemScopes.Any())
                throw new ArgumentException("item is absent in any scope");
            return itemScopes.First();
        }

        #region Implementation of IEnumerable

        public IEnumerator<QuestionnairePropagatedScreenViewModel> GetEnumerator()
        {
            return collection.Select(c => c.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}