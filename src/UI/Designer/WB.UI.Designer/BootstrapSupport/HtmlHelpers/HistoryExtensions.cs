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
using WB.Core.GenericSubdomains.Utils;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.BootstrapSupport.HtmlHelpers
{
    public static class HistoryExtensions
    {
        public static MvcHtmlString FormatQuestionnaireHistoricalRecord(this HtmlHelper helper, UrlHelper urlHelper,
            Guid questionnaireId, QuestionnaireChangeHistoricalRecord record)
        {
            var mainRecord = string.Format("{0} {1} {2}", GetStringRepresentation(record.TargetType),
                BuildQuestionnaireItemLink(helper, urlHelper, questionnaireId, record.TargetId, record.TargetParentId,
                    record.TargetTitle, record.TargetType),
                GetActionStringRepresentations(record.ActionType, record.TargetType,
                    record.HistoricalRecordReferences.Any()));

            foreach (var historicalRecordReference in record.HistoricalRecordReferences)
            {
                mainRecord += string.Format(" {0} {1}",
                    GetStringRepresentation(historicalRecordReference.Type).ToLower(),
                    BuildQuestionnaireItemLink(helper, urlHelper, questionnaireId, historicalRecordReference.Id,
                        historicalRecordReference.ParentId,
                        historicalRecordReference.Title, historicalRecordReference.Type));
            }

            return MvcHtmlString.Create(mainRecord);
        }

        private static MvcHtmlString BuildQuestionnaireItemLink(
            HtmlHelper helper, 
            UrlHelper urlHelper,
            Guid questionnaireId, 
            Guid itemId, 
            Guid? chapterId, 
            string title, 
            QuestionnaireItemType type)
        {
            var quatedTitle = string.Format("\"{0}\"", title);

            if (type == QuestionnaireItemType.Questionnaire)
                return helper.ActionLink(quatedTitle, "Open", "App", new { id = itemId.FormatGuid() }, null);

            if (type == QuestionnaireItemType.Person || !chapterId.HasValue)
                return helper.Label(quatedTitle);

            var url =
                urlHelper.Content(string.Format("~/UpdatedDesigner/app/#/{0}/chapter/{1}/{3}/{2}", questionnaireId.FormatGuid(),
                    chapterId.FormatGuid(), itemId.FormatGuid(), GetQuestionnaireItemTypeStringRepresentationForLink(type)));
            return MvcHtmlString.Create(String.Format("<a href='{0}'>{1}</a>", url, helper.Encode(quatedTitle)));
        }

        private static string GetActionStringRepresentations(QuestionnaireActionType actionType,
            QuestionnaireItemType itemType, bool hasReference)
        {
            var itemsToAdd = new[]
            {
                QuestionnaireItemType.Group, QuestionnaireItemType.Person, QuestionnaireItemType.Question,
                QuestionnaireItemType.Roster, QuestionnaireItemType.StaticText
            };

            switch (actionType)
            {
                case QuestionnaireActionType.Add:
                    return itemsToAdd.Contains(itemType) ? QuestionnaireHistoryResources.added : QuestionnaireHistoryResources.created;
                case QuestionnaireActionType.Clone:
                    return QuestionnaireHistoryResources.cloned + (hasReference ? QuestionnaireHistoryResources.from : "");
                case QuestionnaireActionType.Delete:
                    return QuestionnaireHistoryResources.deleted;
                case QuestionnaireActionType.Update:
                    return QuestionnaireHistoryResources.changed;
                case QuestionnaireActionType.GroupBecameARoster:
                    return QuestionnaireHistoryResources.became_a_roster;
                case QuestionnaireActionType.RosterBecameAGroup:
                    return QuestionnaireHistoryResources.became_a_group;
                case QuestionnaireActionType.Move:
                    return QuestionnaireHistoryResources.moved + (hasReference ? QuestionnaireHistoryResources.to : "");
                case QuestionnaireActionType.Import:
                    return QuestionnaireHistoryResources.imported;
                case QuestionnaireActionType.Replace:
                    return QuestionnaireHistoryResources.replaced;
            }
            return QuestionnaireHistoryResources.unknown;
        }

        private static string GetStringRepresentation(QuestionnaireItemType type)
        {
            return new ResourceManager(typeof(QuestionnaireHistoryResources)).GetString(type.ToString());
        }

        private static string GetQuestionnaireItemTypeStringRepresentationForLink(QuestionnaireItemType type)
        {
            switch (type)
            {
                case QuestionnaireItemType.Group:
                    return "group";
                case QuestionnaireItemType.Question:
                    return "question";
                case QuestionnaireItemType.Roster:
                    return "roster";
                    case QuestionnaireItemType.StaticText:
                    return "static-text";
            }
            return string.Empty;
        }
    }
}