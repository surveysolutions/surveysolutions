using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace AndroidApp.Core.Model.ViewModel.QuestionnaireDetails
{
    public class TemplateCollection : IEnumerable<QuestionnairePropagatedScreenViewModel>
    {
        private readonly Dictionary<Guid, QuestionnairePropagatedScreenViewModel> collection =
            new Dictionary<Guid, QuestionnairePropagatedScreenViewModel>();

        private readonly Dictionary<Guid, Guid[]> scopes =
          new Dictionary<Guid, Guid[]>();
        public void Add(Guid key, QuestionnairePropagatedScreenViewModel value)
        {
            collection.Add(key, value);
        }

        public QuestionnairePropagatedScreenViewModel this[Guid key]
        {
            get { return collection[key]; }
        }
        public void AssignScope(Guid scopeId, IEnumerable<Guid> keys)
        {
            scopes.Add(scopeId, keys.ToArray());
        }

        public IEnumerable<Guid> GetItemsInScope(Guid scopeKey)
        {
            if (!scopes.ContainsKey(scopeKey))
                Enumerable.Empty<QuestionnairePropagatedScreenViewModel>();
            return scopes[scopeKey];
        }

        public IEnumerable<Guid> GetItemsFromScope(Guid scopedItem)
        {
            var itemScopes = scopes.Where(s => s.Value.Contains(scopedItem)).Select(s => s.Key);
            if (!itemScopes.Any())
                return Enumerable.Empty<Guid>();
            return GetItemsInScope(itemScopes.First());
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