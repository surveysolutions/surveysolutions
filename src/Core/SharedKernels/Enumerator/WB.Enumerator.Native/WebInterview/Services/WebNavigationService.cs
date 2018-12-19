using System;
using System.IO;
using System.Linq;
using System.Web;
using HtmlAgilityPack;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Enumerator.Native.WebInterview.Services
{
    public class WebNavigationService : IWebNavigationService
    {
        public string MakeNavigationLinks(string text, Identity entityIdentity, IQuestionnaire questionnaire, IStatefulInterview statefulInterview, bool isReview)
        {
            if (string.IsNullOrEmpty(text)) return text;

            var doc = new HtmlDocument();
            doc.LoadHtml(text);

            var hyperlinks = doc.DocumentNode.SelectNodes("//a");
            if (hyperlinks == null) return text;

            foreach (var hyperlink in hyperlinks)
            {
                var href = hyperlink.Attributes["href"].Value;
                if (Uri.IsWellFormedUriString(href, UriKind.Absolute)) continue;

                hyperlink.Attributes["href"].Value =
                    MakeNavigationLink(href, entityIdentity, questionnaire, statefulInterview, isReview);
            }

            var writer = new StringWriter();
            doc.Save(writer);

            return writer.ToString();
        }

        private static string MakeNavigationLink(string text, Identity entityIdentity, IQuestionnaire questionnaire, IStatefulInterview interview, bool isReview)
        {
            switch (text)
            {
                case "cover":
                    return GenerateInterviewUrl("cover", interview.Id, isReview);
                case "complete":
                    return GenerateInterviewUrl("complete", interview.Id, isReview);
                default:
                {
                    var attachmentId = questionnaire.GetAttachmentIdByName(text);
                    if (attachmentId.HasValue)
                    {
                        var attachment = questionnaire.GetAttachmentById(attachmentId.Value);
                        return GenerateAttachmentUrl(interview.Id, attachment.ContentId);
                    }

                    return MakeNavigationLinkToQuestionOrRoster(text, entityIdentity, questionnaire, interview, isReview) ?? "javascript:void(0);";
                }

            }
        }

        private static string MakeNavigationLinkToQuestionOrRoster(string text, Identity sourceEntity,
            IQuestionnaire questionnaire, IStatefulInterview interview, bool isReview)
        {
            var questionId = questionnaire.GetQuestionIdByVariable(text);
            var rosterId = questionnaire.GetRosterIdByVariableName(text, true);

            if (!questionId.HasValue && !rosterId.HasValue) return null;

            var nearestInterviewEntity = GetNearestInterviewEntity(sourceEntity, interview, questionId ?? rosterId.Value);
            if (nearestInterviewEntity == null) return null;

            if (questionId.HasValue)
                return questionnaire.IsPrefilled(questionId.Value)
                    ? GenerateInterviewUrl("cover", interview.Id, isReview)
                    : GenerateInterviewUrl("section", interview.Id, isReview, interview.GetParentGroup(nearestInterviewEntity), nearestInterviewEntity);

            if (rosterId.HasValue)
                return GenerateInterviewUrl("section", interview.Id, isReview, interview.GetParentGroup(nearestInterviewEntity), nearestInterviewEntity);

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

        private static string GenerateInterviewUrl(string action, Guid interviewId, bool isReview,
            Identity sectionId = null, Identity scrollTo = null)
            => VirtualPathUtility.ToAbsolute(
                $@"~/{(isReview ? "Interview/Review" : "WebInterview")}/{interviewId.FormatGuid()}/{action}{(sectionId == null ? "" : $@"/{sectionId}")}{(scrollTo == null ? "" : $"#{scrollTo}")}");

        private static string GenerateAttachmentUrl(Guid interviewId, string attachmentContentId)
            => VirtualPathUtility.ToAbsolute($"~/api/WebInterviewResources/Content?interviewId={interviewId.FormatGuid()}&contentId={attachmentContentId}");
    }
}
