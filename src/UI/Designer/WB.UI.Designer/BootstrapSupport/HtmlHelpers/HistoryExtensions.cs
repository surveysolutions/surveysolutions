using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Utils;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.BootstrapSupport.HtmlHelpers
{
    public static class HistoryExtensions
    {
        public static MvcHtmlString FormatQuestionnaireHistoricalRecord(this HtmlHelper helper, UrlHelper urlHelper,
           QuestionnaireDocument questionnaireDocument, QuestionnaireChangeHistoricalRecord record)
        {
            var mainRecord = string.Format("{0} {1} {2}", GetStringRepresentation(record.TargetType.ToString()),
                BuildQuestionnaireItemLink(helper, urlHelper, questionnaireDocument, record.TargetId, record.TargetTitle, record.TargetType), GetActionStringRepresentations(record.ActionType, record.TargetType));

            foreach (var historicalRecordReference in record.HistoricalRecordReferences)
            {
                mainRecord += string.Format(" {0} {1}", GetStringRepresentation(historicalRecordReference.Type.ToString()).ToLower(),
                    BuildQuestionnaireItemLink(helper, urlHelper, questionnaireDocument, historicalRecordReference.Id,
                        historicalRecordReference.Title, historicalRecordReference.Type));
            }
              
            return MvcHtmlString.Create(mainRecord);
        }

        private static MvcHtmlString BuildQuestionnaireItemLink(HtmlHelper helper, UrlHelper urlHelper,
            QuestionnaireDocument questionnaireDocument, Guid itemId, string title, QuestionnaireItemType type)
        {
            var quatedTitle = string.Format("\"{0}\"", title);
            if (type == QuestionnaireItemType.Person)
                return helper.Label(quatedTitle);
            if (type == QuestionnaireItemType.Questionnaire)
                return helper.ActionLink(quatedTitle, "Open", "App", new { id = itemId.FormatGuid() }, null);
            var item = questionnaireDocument.FirstOrDefault<IComposite>(g => g.PublicKey == itemId);
            if (item == null)
                return helper.Label(quatedTitle);

            while (item.GetParent().GetType() != typeof(QuestionnaireDocument))
            {
                item = item.GetParent();
            }
            var url =
                urlHelper.Content(string.Format("~/UpdatedDesigner/app/#/{0}/chapter/{1}/{3}/{2}", questionnaireDocument.PublicKey.FormatGuid(),
                    item.PublicKey.FormatGuid(), itemId.FormatGuid(), type.ToString().ToLower()));
            return MvcHtmlString.Create(String.Format("<a href='{0}'>{1}</a>", url, quatedTitle));
        }

        private static string GetActionStringRepresentations(QuestionnaireActionType actionType,
            QuestionnaireItemType itemType)
        {
            var itemsToAdd = new[]
            {
                QuestionnaireItemType.Group, QuestionnaireItemType.Person, QuestionnaireItemType.Question,
                QuestionnaireItemType.Roster, QuestionnaireItemType.StaticText
            };

            switch (actionType)
            {
                case QuestionnaireActionType.Add:
                    return itemsToAdd.Contains(itemType) ? QuestionnaireHistory.added : QuestionnaireHistory.created;
                case QuestionnaireActionType.Clone:
                    return QuestionnaireHistory.cloned_from;
                case QuestionnaireActionType.Delete:
                    return QuestionnaireHistory.deleted;
                case QuestionnaireActionType.Update:
                    return QuestionnaireHistory.changed;
                case QuestionnaireActionType.GroupBecameARoster:
                    return QuestionnaireHistory.became_a_roster;
                case QuestionnaireActionType.RosterBecameAGroup:
                    return QuestionnaireHistory.became_a_group;
                case QuestionnaireActionType.Move:
                    return QuestionnaireHistory.moved_to;
                case QuestionnaireActionType.Import:
                    return QuestionnaireHistory.created;
                case QuestionnaireActionType.Replace:
                    return QuestionnaireHistory.replaced;
            }
            return QuestionnaireHistory.unknown;
        }

        private static string GetStringRepresentation(string type)
        {
            return new ResourceManager(typeof(QuestionnaireHistory)).GetString(type);
        }
    }
}