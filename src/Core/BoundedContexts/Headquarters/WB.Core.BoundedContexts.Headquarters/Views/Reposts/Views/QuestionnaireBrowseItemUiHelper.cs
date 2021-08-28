using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views
{
    public static class QuestionnaireBrowseItemUiHelper
    {
        public static List<QuestionnaireVersionsComboboxViewItem> GetQuestionnaireComboboxViewItems(
            this List<QuestionnaireBrowseItem> questionnaireBrowseItems)
        {
            return questionnaireBrowseItems
                .GroupBy(questionnaire => new { questionnaire.QuestionnaireId})
                .Select(g => new QuestionnaireVersionsComboboxViewItem
                {
                    Key = g.Key.QuestionnaireId.ToString(),
                    Value = g.OrderByDescending(v => v.Version).First().Title,
                    Versions = g.OrderByDescending(v => v.Version)
                        .Select(v => new ComboboxViewItem { Key = v.Version.ToString(), Value = $"ver. {v.Version}" })
                        .ToList(),
                })
                .OrderBy(q => q.Value, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
