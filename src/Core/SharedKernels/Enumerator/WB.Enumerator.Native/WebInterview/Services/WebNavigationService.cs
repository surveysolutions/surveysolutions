using System;
using System.IO;
using System.Linq;
using HtmlAgilityPack;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.UI.Shared.Web.Services;

namespace WB.Enumerator.Native.WebInterview.Services
{
    public class WebNavigationService : IWebNavigationService
    {
        const string DefaultNavigationLink = "javascript:void(0);";

        private readonly IVirtualPathService virtualPathService;

        public WebNavigationService(IVirtualPathService virtualPathService)
        {
            this.virtualPathService = virtualPathService;
        }

        public string MakeNavigationLinks(string text, Identity entityIdentity, IQuestionnaire questionnaire,
            IStatefulInterview statefulInterview, string virtualDirectoryName)
            => this.ReplaceNavigationLinks(text, hyperlink =>
            {
                var href = hyperlink.Attributes["href"].Value;
                if (Uri.IsWellFormedUriString(href, UriKind.Absolute)) return;

                hyperlink.Attributes["href"].Value =
                    MakeNavigationLink(href, entityIdentity, questionnaire, statefulInterview, virtualDirectoryName);
            });

        public string ResetNavigationLinksToDefault(string text)
            => this.ReplaceNavigationLinks(text, hyperlink => hyperlink.Attributes["href"].Value = DefaultNavigationLink);

        private string ReplaceNavigationLinks(string text, Action<HtmlNode> onReplace)
        {
            if (string.IsNullOrEmpty(text)) return text;

            var doc = new HtmlDocument();
            doc.LoadHtml(text);

            var hyperlinks = doc.DocumentNode.SelectNodes("//a");
            if (hyperlinks == null) return text;

            foreach (var hyperlink in hyperlinks)
                onReplace.Invoke(hyperlink);

            using var writer = new StringWriter();
            doc.Save(writer);
            return writer.ToString();
        }

        private string MakeNavigationLink(string text, Identity entityIdentity, IQuestionnaire questionnaire, IStatefulInterview interview, string virtualDirectoryName)
        {
            switch (text)
            {
                case "cover":
                    return GenerateInterviewUrl("cover", interview.Id, virtualDirectoryName);
                case "complete":
                    return GenerateInterviewUrl("complete", interview.Id, virtualDirectoryName);
                default:
                {
                    var attachmentId = questionnaire.GetAttachmentIdByName(text);
                    if (attachmentId.HasValue)
                    {
                        var attachment = questionnaire.GetAttachmentById(attachmentId.Value);
                        return GenerateAttachmentUrl(interview.Id, attachment.ContentId);
                    }

                    return MakeNavigationLinkToQuestionOrRoster(text, entityIdentity, questionnaire, interview, virtualDirectoryName) ?? DefaultNavigationLink;
                }
            }
        }

        private string MakeNavigationLinkToQuestionOrRoster(string text, Identity sourceEntity,
            IQuestionnaire questionnaire, IStatefulInterview interview, string virtualDirectoryName)
        {
            var questionId = questionnaire.GetQuestionIdByVariable(text);
            var rosterOrGroupId = questionnaire.GetRosterIdByVariableName(text, true) ?? questionnaire.GetSectionIdByVariable(text);

            if (!questionId.HasValue && !rosterOrGroupId.HasValue) return null;

            var nearestInterviewEntity = GetNearestInterviewEntity(sourceEntity, interview, questionId ?? rosterOrGroupId.Value);
            if (nearestInterviewEntity == null) return null;

            if (questionId.HasValue)
                return questionnaire.IsPrefilled(questionId.Value) && !questionnaire.IsCoverPageSupported
                    ? GenerateInterviewUrl("Cover", interview.Id, virtualDirectoryName)
                    : GenerateInterviewUrl("Section", interview.Id, virtualDirectoryName, interview.GetParentGroup(nearestInterviewEntity), nearestInterviewEntity);

            if (rosterOrGroupId.HasValue)
                return GenerateInterviewUrl("Section", interview.Id, virtualDirectoryName, interview.GetParentGroup(nearestInterviewEntity) ?? nearestInterviewEntity, nearestInterviewEntity);

            return null;
        }

        private static Identity GetNearestInterviewEntity(Identity sourceEntity, IStatefulInterview interview, Guid questionOrRosterId)
        {
            Identity nearestInterviewEntity = null;
            var interviewEntities = interview.GetAllIdentitiesForEntityId(questionOrRosterId)
                .Where(interview.IsEnabled).ToArray();

            if (interviewEntities.Length == 1)
                nearestInterviewEntity = interviewEntities[0];
            else
            {
                var entitiesInTheSameOrDeeperRoster = interviewEntities.Where(x =>
                    x.RosterVector.Identical(sourceEntity.RosterVector,
                        sourceEntity.RosterVector.Length)).ToArray();

                if (entitiesInTheSameOrDeeperRoster.Any())
                    nearestInterviewEntity = entitiesInTheSameOrDeeperRoster.FirstOrDefault();
                else
                {
                    var sourceEntityParentRosterVectors =
                        interview.GetParentGroups(sourceEntity).Select(x => x.RosterVector).ToArray();

                    nearestInterviewEntity = interviewEntities.FirstOrDefault(x =>
                                                 x.Id == (questionOrRosterId) &&
                                                 sourceEntityParentRosterVectors.Contains(x.RosterVector)) ??
                                             interviewEntities.FirstOrDefault();
                }
            }

            return nearestInterviewEntity;
        }

        private string GenerateInterviewUrl(string action, Guid interviewId, string virtualDirectoryName, Identity sectionId = null, Identity scrollTo = null)
        {
            var relativeUrl = $@"~/{virtualDirectoryName}/{interviewId.FormatGuid()}/{action}{(sectionId == null ? "" : $@"/{sectionId}")}{(scrollTo == null ? "" : $"#{scrollTo}")}";
            return this.virtualPathService.GetRelatedToRootPath(relativeUrl);
        }

        private string GenerateAttachmentUrl(Guid interviewId, string attachmentContentId)
            => this.virtualPathService.GetAbsolutePath($"~/api/WebInterviewResources/Content?interviewId={interviewId.FormatGuid()}&contentId={attachmentContentId}");
    }
}
