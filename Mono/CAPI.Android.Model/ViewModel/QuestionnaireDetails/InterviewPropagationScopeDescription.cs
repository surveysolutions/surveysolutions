using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails
{
    public class InterviewPropagationScopeDescription
    {
        private readonly Dictionary<Guid, QuestionnairePropagatedScreenViewModel> propagateGroups =
            new Dictionary<Guid, QuestionnairePropagatedScreenViewModel>();

        private readonly Dictionary<Guid, IList<Guid>> scopesWithTriggers =
          new Dictionary<Guid, IList<Guid>>();

        public void AddTemplateOfPropagatedScreen(Guid key, QuestionnairePropagatedScreenViewModel value)
        {
            this.propagateGroups.Add(key, value);
            if (!this.scopesWithTriggers.Where(s => s.Value.Contains(key)).Select(s => s.Key).Any())
                this.scopesWithTriggers.Add(key, new List<Guid> {key});
        }

        public QuestionnairePropagatedScreenViewModel GetTemplateOfPropagatedScreen(Guid key)
        {
            return this.propagateGroups[key];
        }

        public void CreateScopeOfPropagatedScreens(Guid scopeId, IEnumerable<Guid> keys)
        {
            foreach (var guid in keys)
            {
                var scopeBy = this.scopesWithTriggers.Where(s => s.Value.Contains(guid)).Select(s => s.Key);
                if (!scopeBy.Any())
                    continue;
                var currentScope = scopeBy.First();
                var itemsInCurrentScope = this.scopesWithTriggers[currentScope];
                if (itemsInCurrentScope.Count == 1)
                {
                    this.scopesWithTriggers.Remove(currentScope);
                }
                else
                {
                    itemsInCurrentScope.Remove(guid);
                }
            }
            this.scopesWithTriggers.Add(scopeId, keys.ToList());
        }

        public IEnumerable<Guid> GetTemplatesOfPropagatedScreensInScope(Guid scopeKey)
        {
            if (!this.scopesWithTriggers.ContainsKey(scopeKey))
                throw new ArgumentException("item is absent in any scope");
            return this.scopesWithTriggers[scopeKey];
        }

        public IEnumerable<Guid> GetScreenSiblingsByPropagationLevel(Guid scopedItem)
        {
            var itemScopes = this.GetScopeOfPropagatedScreen(scopedItem);
            return this.GetTemplatesOfPropagatedScreensInScope(itemScopes);
        }

        public Guid GetScopeOfPropagatedScreen(Guid itemKey)
        {
            var itemScopes = this.scopesWithTriggers.Where(s => s.Value.Contains(itemKey)).Select(s => s.Key);
            if (!itemScopes.Any())
                throw new ArgumentException("item is absent in any scope");
            return itemScopes.First();
        }

        public IEnumerable<QuestionnairePropagatedScreenViewModel> GetAllPropagatedScreenTemplates()
        {
            return this.propagateGroups.Select(c => c.Value);
        }
    }
}