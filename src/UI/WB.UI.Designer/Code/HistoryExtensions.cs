using System;
using System.Linq;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Code
{
    public static class HistoryExtensions
    {
        public static HtmlString FormatQuestionnaireHistoricalRecord(this IHtmlHelper helper, IUrlHelper urlHelper,
            Guid questionnaireId, QuestionnaireChangeHistoricalRecord record)
        {
            var recordLink = BuildQuestionnaireItemLink(helper, urlHelper, questionnaireId, record.TargetId,
                record.TargetParentId, record.TargetTitle, true, record.TargetType);

            var text = string.Empty;

            switch (record.ActionType)
            {
                case QuestionnaireActionType.Mark:
                    switch (record.TargetType)
                    {
                        case QuestionnaireItemType.Translation:
                            var language = record.TargetTitle ?? QuestionnaireHistoryResources.Translation_Original;
                            text = string.Format(QuestionnaireHistoryResources.Translation_DefaultTranslationSet, language);
                            break;
                    }
                    break;
                case QuestionnaireActionType.ReplaceAllTexts:
                    text = string.Format(QuestionnaireHistoryResources.TextsReplaced, record.TargetTitle,
                        record.TargetNewTitle, record.AffectedEntries);
                    break;
                case QuestionnaireActionType.Revert:
                    text = ToRevertQuestionnireMessage(record);
                    break;
                case QuestionnaireActionType.MigrateToNewVersion:
                    text = QuestionnaireHistoryResources.MigratedNewCoverSupport;
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
                            case QuestionnaireItemType.Categories:
                            case QuestionnaireItemType.CriticalityCondition:
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
                            case QuestionnaireItemType.Metadata:
                                text = string.Format(QuestionnaireHistoryResources.ResourceManager.GetString($"{record.TargetType}_{record.ActionType}") ?? string.Empty, recordLink);
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
                            case QuestionnaireItemType.Questionnaire:
                                text = GetResource(record.TargetType, record.ActionType).FormatString(
                                    record.TargetTitle
                                );
                                break;
                        }
                    }
                    break;
                case QuestionnaireActionType.ImportToHq:
                {
                    
                    string siteHost = (record.TargetNewTitle ?? record.TargetTitle) ?? String.Empty;

                    var indexOfOurDomain = siteHost.IndexOf(".mysurvey.solutions");
                    siteHost = indexOfOurDomain > 0
                        ? siteHost.Substring(0, indexOfOurDomain)
                        : siteHost;

                    if(record.HqVersion != null)
                    {
                        siteHost += " v" + record.HqVersion;
                    }

                    var questionnaireVersion = $"ver. {record.HqQuestionnaireVersion}";

                    text = string.Format(QuestionnaireHistoryResources.Questionnaire_ImportToHq,
                        siteHost,
                        record.HqUserName,
                        questionnaireVersion);
                }
                break;
            }
            return new HtmlString(text);
        }

        private static string ToMoveMessage(IHtmlHelper helper, IUrlHelper urlHelper, Guid questionnaireId,
            QuestionnaireChangeHistoricalRecord record, HtmlString recordLink)
        {
            var historicalRecordReference = record.HistoricalRecordReferences.FirstOrDefault();
            var targetType = historicalRecordReference?.Type.ToString() ?? "Section";
            return string.Format(
                QuestionnaireHistoryResources.ResourceManager.GetString($"{record.TargetType}_{record.ActionType}_To_{targetType}") ?? string.Empty,
                recordLink,
                historicalRecordReference == null ? HtmlString.Empty :
                BuildQuestionnaireItemLink(helper, urlHelper, questionnaireId,
                    historicalRecordReference.Id, historicalRecordReference.ParentId,
                    historicalRecordReference.Title, historicalRecordReference.IsExist,
                    historicalRecordReference.Type));
        }

        private static string? GetResource(QuestionnaireItemType itemType, QuestionnaireActionType actionType) =>
            QuestionnaireHistoryResources.ResourceManager.GetString($"{itemType}_{actionType}");

        private static string  ToCloneMessage(IHtmlHelper helper, IUrlHelper urlHelper, Guid questionnaireId,
            QuestionnaireChangeHistoricalRecord record, HtmlString recordLink)
        {
            var historicalRecordReference = record.HistoricalRecordReferences.FirstOrDefault();

            return string.Format(
                GetResource(record.TargetType, record.ActionType) ?? string.Empty,
                recordLink,
                historicalRecordReference == null ? HtmlString.Empty :
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

        private static HtmlString BuildQuestionnaireItemLink(IHtmlHelper helper, IUrlHelper urlHelper,
            Guid questionnaireId, Guid itemId, Guid? chapterId, string? title, bool isExist, QuestionnaireItemType type)
        {
            title = title?.Replace("Empty macro added", "")?.Replace("Empty lookup table added", "");

            var entityTitle = helper.Encode(string.IsNullOrEmpty(title)
                ? $"<<{QuestionnaireHistoryResources.NoTitle}>>"
                : $"\"{title}\"");

            if (!isExist)
                return new HtmlString(entityTitle);

            if (type == QuestionnaireItemType.Questionnaire)
                return new HtmlString($"<a href='{urlHelper.Content($"~/q/details/{itemId.FormatGuid()}")}'>{entityTitle}</a>");

            if (type == QuestionnaireItemType.Person || !chapterId.HasValue)
                return new HtmlString(entityTitle);

            var url = urlHelper.Content(string.Format("~/q/details/{0}/chapter/{1}/{3}/{2}",
                questionnaireId.FormatGuid(), chapterId.FormatGuid(), itemId.FormatGuid(),
                GetNavigationItemType(type)));

            return new HtmlString($"<a href='{url}'>{entityTitle}</a>");
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
