using System;
using System.Linq;
using System.Resources;
using System.Runtime.Remoting;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.BootstrapSupport.HtmlHelpers
{
    public static class HistoryExtensions
    {
        public static MvcHtmlString FormatQuestionnaireHistoricalRecord(this HtmlHelper helper, UrlHelper urlHelper,
            Guid questionnaireId, QuestionnaireChangeHistoricalRecord record)
        {
            if (record.TargetType == QuestionnaireItemType.Translation)
            {
                var translationRecord = GetFormattedHistoricalRecordForTranslation(record);
                if (!string.IsNullOrWhiteSpace(translationRecord))
                    return MvcHtmlString.Create(translationRecord);
            }

            if (record.TargetType == QuestionnaireItemType.Attachment)
            {
                var attachmentRecord = GetFormattedHistoricalRecordForAttachment(record);
                if (!string.IsNullOrWhiteSpace(attachmentRecord))
                    return MvcHtmlString.Create(attachmentRecord);
            }

            if (record.TargetType == QuestionnaireItemType.Macro)
            {
                var macrosRecord = GetFormattedHistoricalRecordForMacros(record);
                if (!string.IsNullOrWhiteSpace(macrosRecord))
                    return MvcHtmlString.Create(macrosRecord);
            }

            if (record.TargetType == QuestionnaireItemType.LookupTable)
            {
                var lookupTableRecord = GetFormattedHistoricalRecordForLookupTable(record);
                if (!string.IsNullOrWhiteSpace(lookupTableRecord))
                    return MvcHtmlString.Create(lookupTableRecord);
            }

            if (record.TargetType == QuestionnaireItemType.Variable)
            {
                var variableRecord = GetFormattedHistoricalRecordForVariable(record);
                if (!string.IsNullOrWhiteSpace(variableRecord))
                    return MvcHtmlString.Create(variableRecord);
            }

            if (record.ActionType == QuestionnaireActionType.ReplaceAllTexts)
            {
                var replaceRecord = GetFormattedHistoricalRecordForReplaceTexts(record);
                if (!string.IsNullOrWhiteSpace(replaceRecord))
                    return MvcHtmlString.Create(replaceRecord);
            }

            if (record.ActionType == QuestionnaireActionType.Revert)
            {
                var revertRecord = GetFormattedHistoricalRecordForRevertQuestionnire(record);
                if (!string.IsNullOrWhiteSpace(revertRecord))
                    return MvcHtmlString.Create(revertRecord);
            }

            var localizedNameOfItemBeingInAction = GetStringRepresentation(record.TargetType);

            var questionnaireItemLinkWithTitle = BuildQuestionnaireItemLink(helper, urlHelper, questionnaireId, record.TargetId, record.TargetParentId,
                record.TargetTitle, true, record.TargetType);

            var localizedActionRepresentation = GetActionStringRepresentation(record.ActionType, record.TargetType, record.HistoricalRecordReferences.Any());

            var mainRecord = $"{localizedNameOfItemBeingInAction} {questionnaireItemLinkWithTitle} {localizedActionRepresentation}";

            foreach (var historicalRecordReference in record.HistoricalRecordReferences)
            {
                mainRecord += string.Format(" {0} {1}",
                    GetStringRepresentation(historicalRecordReference.Type)?.ToLower(),
                    BuildQuestionnaireItemLink(helper, urlHelper, questionnaireId, historicalRecordReference.Id,
                        historicalRecordReference.ParentId,
                        historicalRecordReference.Title, historicalRecordReference.IsExist,
                        historicalRecordReference.Type));
            }

            return MvcHtmlString.Create(mainRecord);
        }

        private static string GetFormattedHistoricalRecordForReplaceTexts(QuestionnaireChangeHistoricalRecord record)
        {
            return string.Format(QuestionnaireHistoryResources.TextsReplaced, record.TargetTitle, record.TargetNewTitle,
                record.AffectedEntries);
        }

        private static string GetFormattedHistoricalRecordForRevertQuestionnire(QuestionnaireChangeHistoricalRecord record)
        {
            var questionnireTitle = record.TargetTitle;
            var questionnireDateTime = record.TargetDateTime;
            return questionnireDateTime.HasValue
                ? string.Format(QuestionnaireHistoryResources.reverted_to, questionnireTitle, questionnireDateTime.Value.ToString("s"))
                : string.Format(QuestionnaireHistoryResources.reverted, questionnireTitle);
        }

        private static string GetFormattedHistoricalRecordForVariable(QuestionnaireChangeHistoricalRecord record)
        {
            switch (record.ActionType)
            {
                case QuestionnaireActionType.Add:
                    if (string.IsNullOrEmpty(record.TargetTitle))
                        return QuestionnaireHistoryResources.VariableAdded;
                    return string.Format(QuestionnaireHistoryResources.VariableAddedWithName, record.TargetTitle);
                case QuestionnaireActionType.Delete:
                    if (string.IsNullOrEmpty(record.TargetTitle))
                        return QuestionnaireHistoryResources.VariableDeleted;
                    return string.Format(QuestionnaireHistoryResources.VariableDeletedWithName, record.TargetTitle);
                case QuestionnaireActionType.Update:
                    if (string.IsNullOrEmpty(record.TargetTitle))
                        return QuestionnaireHistoryResources.VariableUpdated;
                    return string.Format(QuestionnaireHistoryResources.VariableUpdatedWithName, record.TargetTitle);
            }
            return null;
        }

        private static string GetFormattedHistoricalRecordForAttachment(QuestionnaireChangeHistoricalRecord record)
        {
            switch (record.ActionType)
            {
                case QuestionnaireActionType.Delete:
                    return string.Format(QuestionnaireHistoryResources.Attachment_Deleted, record.TargetTitle);
                case QuestionnaireActionType.Update:
                    return string.Format(QuestionnaireHistoryResources.Attachment_Updated, record.TargetTitle);
            }
            return null;
        }

        private static string GetFormattedHistoricalRecordForTranslation(QuestionnaireChangeHistoricalRecord record)
        {
            switch (record.ActionType)
            {
                case QuestionnaireActionType.Delete:
                    return string.Format(QuestionnaireHistoryResources.Translation_Deleted, record.TargetTitle);
                case QuestionnaireActionType.Update:
                    return string.Format(QuestionnaireHistoryResources.Translation_Updated, record.TargetTitle);
            }
            return null;
        }

        private static string GetFormattedHistoricalRecordForLookupTable(QuestionnaireChangeHistoricalRecord record)
        {
            switch (record.ActionType)
            {
                case QuestionnaireActionType.Add:
                    return QuestionnaireHistoryResources.LookupTable_EmptyTableAdded;
                case QuestionnaireActionType.Delete:
                    if (string.IsNullOrEmpty(record.TargetTitle))
                        return QuestionnaireHistoryResources.LookupTable_EmptyTableDeleted;
                    return string.Format(QuestionnaireHistoryResources.LookupTable_Deleted, record.TargetTitle);
                case QuestionnaireActionType.Update:
                    if (string.IsNullOrEmpty(record.TargetTitle))
                        return QuestionnaireHistoryResources.LookupTable_EmptyTableUpdated;
                    return string.Format(QuestionnaireHistoryResources.LookupTable_Updated, record.TargetTitle);
            }
            return null;
        }

        private static string GetFormattedHistoricalRecordForMacros(QuestionnaireChangeHistoricalRecord record)
        {
            switch (record.ActionType)
            {
                case QuestionnaireActionType.Add:
                    return QuestionnaireHistoryResources.Macro_EmptyMacroAdded;
                case QuestionnaireActionType.Delete:
                    if (string.IsNullOrEmpty(record.TargetTitle))
                        return QuestionnaireHistoryResources.Macro_EmptyMacroDeleted;
                    return string.Format(QuestionnaireHistoryResources.Macro_Deleted, record.TargetTitle);
                case QuestionnaireActionType.Update:
                    if (string.IsNullOrEmpty(record.TargetTitle))
                        return QuestionnaireHistoryResources.Macro_EmptyMacroUpdated;
                    return string.Format(QuestionnaireHistoryResources.Macro_Updated, record.TargetTitle);
            }
            return null;
        }

        private static MvcHtmlString BuildQuestionnaireItemLink(
            HtmlHelper helper, 
            UrlHelper urlHelper,
            Guid questionnaireId, 
            Guid itemId, 
            Guid? chapterId, 
            string title,
            bool isExist,
            QuestionnaireItemType type)
        {
            var entityTitle = $"\"{helper.Encode(title)}\"";

            if (!isExist)
                return MvcHtmlString.Create(entityTitle);

            if (type == QuestionnaireItemType.Questionnaire)
                return helper.ActionLink(entityTitle, "Details", "Questionnaire", new { id = itemId.FormatGuid() }, null);

            if (type == QuestionnaireItemType.Person || !chapterId.HasValue)
                return MvcHtmlString.Create(entityTitle);

            var url =
                urlHelper.Content(string.Format("~/questionnaire/details/{0}/chapter/{1}/{3}/{2}", questionnaireId.FormatGuid(),
                    chapterId.FormatGuid(), itemId.FormatGuid(), GetQuestionnaireItemTypeStringRepresentationForLink(type)));
            return MvcHtmlString.Create($"<a href='{url}'>{entityTitle}</a>");
        }

        private static string GetActionStringRepresentation(QuestionnaireActionType actionType,
            QuestionnaireItemType itemType, bool hasReference)
        {
            var itemsToAdd = new[]
            {
                QuestionnaireItemType.Group, QuestionnaireItemType.Person, QuestionnaireItemType.Question,
                QuestionnaireItemType.Roster, QuestionnaireItemType.StaticText, QuestionnaireItemType.Variable, 
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
                case QuestionnaireItemType.Variable:
                    return "variable";
            }
            return string.Empty;
        }
    }
}