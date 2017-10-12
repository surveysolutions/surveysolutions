using System;
using System.Linq;
using System.Web.Mvc;
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
            var recordLink = BuildQuestionnaireItemLink(helper, urlHelper, questionnaireId, record.TargetId,
                record.TargetParentId, record.TargetTitle, true, record.TargetType);

            var text = string.Empty;

            switch (record.ActionType)
            {
                case QuestionnaireActionType.ReplaceAllTexts:
                    text = string.Format(QuestionnaireHistoryResources.TextsReplaced, record.TargetTitle,
                        record.TargetNewTitle, record.AffectedEntries);
                    break;
                case QuestionnaireActionType.Revert:
                    text = ToRevertQuestionnireMessage(record);
                    break;
                case QuestionnaireActionType.Add:
                case QuestionnaireActionType.Update:
                case QuestionnaireActionType.Delete:
                case QuestionnaireActionType.GroupBecameARoster:
                case QuestionnaireActionType.RosterBecameAGroup:
                case QuestionnaireActionType.Import:
                case QuestionnaireActionType.Replace:
                    {
                        switch (record.TargetType)
                        {
                            case QuestionnaireItemType.Translation:
                            case QuestionnaireItemType.Attachment:
                            case QuestionnaireItemType.Macro:
                            case QuestionnaireItemType.LookupTable:
                            case QuestionnaireItemType.Variable:
                            case QuestionnaireItemType.Section:
                            case QuestionnaireItemType.Question:
                            case QuestionnaireItemType.Roster:
                            case QuestionnaireItemType.StaticText:
                            case QuestionnaireItemType.Questionnaire:
                            case QuestionnaireItemType.Person:
                                text = string.Format(QuestionnaireHistoryResources.ResourceManager.GetString($"{record.TargetType}_{record.ActionType}"), recordLink);
                                break;
                        }
                    }
                    break;
                case QuestionnaireActionType.Clone:
                    {
                    switch (record.TargetType)
                    {
                        case QuestionnaireItemType.Variable:
                        case QuestionnaireItemType.Section:
                        case QuestionnaireItemType.Question:
                        case QuestionnaireItemType.Roster:
                        case QuestionnaireItemType.StaticText:
                        case QuestionnaireItemType.Questionnaire:
                            text = ToCloneMessage(helper, urlHelper, questionnaireId, record, recordLink);
                            break;
                    }

                }
                    break;
                case QuestionnaireActionType.Move:
                {
                    switch (record.TargetType)
                    {
                        case QuestionnaireItemType.Variable:
                        case QuestionnaireItemType.Section:
                        case QuestionnaireItemType.Question:
                        case QuestionnaireItemType.Roster:
                        case QuestionnaireItemType.StaticText:
                            text = ToMoveMessage(helper, urlHelper, questionnaireId, record, recordLink);
                            break;
                    }

                }
                    break;
            }
            return MvcHtmlString.Create(text);
        }

        private static string ToMoveMessage(HtmlHelper helper, UrlHelper urlHelper, Guid questionnaireId,
            QuestionnaireChangeHistoricalRecord record, MvcHtmlString recordLink)
        {
            var historicalRecordReference = record.HistoricalRecordReferences.FirstOrDefault();

            return string.Format(
                QuestionnaireHistoryResources.ResourceManager.GetString($"{record.TargetType}_{record.ActionType}_To_{historicalRecordReference.Type}"),
                recordLink,
                BuildQuestionnaireItemLink(helper, urlHelper, questionnaireId,
                    historicalRecordReference.Id, historicalRecordReference.ParentId,
                    historicalRecordReference.Title, historicalRecordReference.IsExist,
                    historicalRecordReference.Type));
        }

        private static string ToCloneMessage(HtmlHelper helper, UrlHelper urlHelper, Guid questionnaireId,
            QuestionnaireChangeHistoricalRecord record, MvcHtmlString recordLink)
        {
            var historicalRecordReference = record.HistoricalRecordReferences.FirstOrDefault();

            return string.Format(
                QuestionnaireHistoryResources.ResourceManager.GetString($"{record.TargetType}_{record.ActionType}"),
                recordLink,
                BuildQuestionnaireItemLink(helper, urlHelper, questionnaireId,
                    historicalRecordReference.Id, historicalRecordReference.ParentId,
                    historicalRecordReference.Title, historicalRecordReference.IsExist,
                    historicalRecordReference.Type));
        }

        private static string ToRevertQuestionnireMessage(QuestionnaireChangeHistoricalRecord record)
        {
            var questionnireTitle = record.TargetTitle;
            var questionnireDateTime = record.TargetDateTime;
            return questionnireDateTime.HasValue
                ? string.Format(QuestionnaireHistoryResources.reverted_to, questionnireTitle, questionnireDateTime.Value.ToString("s"))
                : string.Format(QuestionnaireHistoryResources.reverted, questionnireTitle);
        }

        private static MvcHtmlString BuildQuestionnaireItemLink(HtmlHelper helper, UrlHelper urlHelper,
            Guid questionnaireId, Guid itemId, Guid? chapterId, string title, bool isExist, QuestionnaireItemType type)
        {
            title = title?.Replace("Empty macro added", "")?.Replace("Empty lookup table added", "");

            var entityTitle = helper.Encode(string.IsNullOrEmpty(title)
                ? $"<<{QuestionnaireHistoryResources.NoTitle}>>"
                : $"\"{title}\"");

            if (!isExist)
                return MvcHtmlString.Create(entityTitle);

            if (type == QuestionnaireItemType.Questionnaire)
                return MvcHtmlString.Create($"<a href='{urlHelper.Content($"~/questionnaire/details/{itemId.FormatGuid()}")}'>{entityTitle}</a>");

            if (type == QuestionnaireItemType.Person || !chapterId.HasValue)
                return MvcHtmlString.Create(entityTitle);

            var url = urlHelper.Content(string.Format("~/questionnaire/details/{0}/chapter/{1}/{3}/{2}",
                questionnaireId.FormatGuid(), chapterId.FormatGuid(), itemId.FormatGuid(),
                GetNavigationItemType(type)));

            return MvcHtmlString.Create($"<a href='{url}'>{entityTitle}</a>");
        }

        private static string GetNavigationItemType(QuestionnaireItemType type)
        {
            switch (type)
            {
                case QuestionnaireItemType.Section:
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