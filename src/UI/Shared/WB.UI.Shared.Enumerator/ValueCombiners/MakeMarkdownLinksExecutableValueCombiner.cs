using System;
using System.Collections.Generic;
using System.Linq;
using Android.OS;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Java.Lang;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.UI.Shared.Enumerator.ValueCombiners
{
    public class MakeMarkdownLinksExecutableValueCombiner : BaseValueCombiner<ICharSequence>
    {
        protected override int ExpectedParamsCount => 2;

        protected override ICharSequence GetValue(List<object> values)
        {
            string htmlText = values[0]?.ToString() ?? string.Empty;
            var interviewEntity = (IInterviewEntity) values[1];

            if (interviewEntity == null) return new SpannableString(htmlText);

            ICharSequence sequence = Build.VERSION.SdkInt >= BuildVersionCodes.N
                ? Html.FromHtml(htmlText, FromHtmlOptions.ModeLegacy)
                : Html.FromHtml(htmlText);

            var strBuilder = new SpannableStringBuilder(sequence);

            var urlSpans = strBuilder.GetSpans(0, sequence.Length(), Class.FromType(typeof(URLSpan)));
            foreach (URLSpan span in urlSpans)
            {
                if (!Uri.IsWellFormedUriString(span.URL, UriKind.Absolute))
                    this.MakeNavigationLink(strBuilder, span, interviewEntity);
            }

            return strBuilder;
        }

        protected void MakeNavigationLink(SpannableStringBuilder strBuilder, URLSpan span, IInterviewEntity interviewEntity)
        {
            int start = strBuilder.GetSpanStart(span);
            int end = strBuilder.GetSpanEnd(span);
            var flags = strBuilder.GetSpanFlags(span);

            var navigationSpan = new NavigateToEntitySpan(this.NavigateToEntity, span.URL, interviewEntity);

            strBuilder.SetSpan(navigationSpan, start, end, flags);
            strBuilder.RemoveSpan(span);
        }

        private void NavigateToEntity(string entityVariable, IInterviewEntity sourceEntity)
        {
            if(sourceEntity.NavigationState == null) return;
            if (string.IsNullOrEmpty(entityVariable)) return;

            entityVariable = entityVariable.ToLower();

            if (entityVariable == "cover")
                sourceEntity.NavigationState.NavigateTo(NavigationIdentity.CreateForCoverScreen());
            else if (entityVariable == "complete")
                sourceEntity.NavigationState.NavigateTo(NavigationIdentity.CreateForCompleteScreen());
            else if (entityVariable == "overview")
                sourceEntity.NavigationState.NavigateTo(NavigationIdentity.CreateForOverviewScreen());
            else
            {
                var interview = ServiceLocator.Current.GetInstance<IStatefulInterviewRepository>().Get(sourceEntity.InterviewId);
                if (interview == null) return;

                var questionnaire = ServiceLocator.Current.GetInstance<IQuestionnaireStorage>()
                    .GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
                if (questionnaire == null) return;

                var attachmentId = questionnaire.GetAttachmentIdByName(entityVariable);
                if (attachmentId.HasValue)
                    NavigateToAttachment(sourceEntity, attachmentId, questionnaire);
                else
                    NavigateToQuestionOrRosterOrSection(entityVariable, sourceEntity, questionnaire, interview);
            }
        }

        private static void NavigateToQuestionOrRosterOrSection(string entityVariable, IInterviewEntity sourceEntity,
            IQuestionnaire questionnaire, IStatefulInterview interview)
        {
            var questionId = questionnaire.GetQuestionIdByVariable(entityVariable);
            Guid? rosterOrGroupId = null;

            if (questionId == null)
                rosterOrGroupId = questionnaire.GetRosterIdByVariableName(entityVariable, true) 
                                  ?? questionnaire.GetSectionIdByVariable(entityVariable);

            if (!questionId.HasValue && !rosterOrGroupId.HasValue) return;

            Identity nearestInterviewEntity = null;
            var interviewEntities = interview.GetAllIdentitiesForEntityId(questionId ?? rosterOrGroupId.Value)
                .Where(interview.IsEnabled).ToArray();

            if (interviewEntities.Length == 1)
                nearestInterviewEntity = interviewEntities[0];
            else
            {
                var entitiesInTheSameOrDeeperRoster = interviewEntities.Where(x =>
                    x.RosterVector.Identical(sourceEntity.Identity.RosterVector,
                        sourceEntity.Identity.RosterVector.Length)).ToArray();

                if (entitiesInTheSameOrDeeperRoster.Any())
                    nearestInterviewEntity = entitiesInTheSameOrDeeperRoster.FirstOrDefault();
                else
                {
                    var sourceEntityParentRosterVectors =
                        interview.GetParentGroups(sourceEntity.Identity).Select(x => x.RosterVector).ToArray();

                    nearestInterviewEntity = interviewEntities.FirstOrDefault(x =>
                                                 x.Id == (questionId ?? rosterOrGroupId.Value) &&
                                                 sourceEntityParentRosterVectors.Contains(x.RosterVector)) ??
                                             interviewEntities.FirstOrDefault();
                }
            }

            if (nearestInterviewEntity == null) return;

            if (questionId.HasValue)
            {
                sourceEntity.NavigationState.NavigateTo(new NavigationIdentity
                {
                    TargetScreen = questionnaire.IsPrefilled(questionId.Value)
                        ? ScreenType.Cover
                        : ScreenType.Group,
                    TargetGroup = interview.GetParentGroup(nearestInterviewEntity),
                    AnchoredElementIdentity = nearestInterviewEntity
                });
            }
            else if (rosterOrGroupId.HasValue)
            {
                sourceEntity.NavigationState.NavigateTo(new NavigationIdentity
                {
                    TargetScreen = ScreenType.Group,
                    TargetGroup = interview.GetParentGroup(nearestInterviewEntity) ?? nearestInterviewEntity, //for section(chapter) it would be opened
                    AnchoredElementIdentity = nearestInterviewEntity
                });
            }
        }

        private void NavigateToAttachment(IInterviewEntity sourceEntity, Guid? attachmentId, IQuestionnaire questionnaire)
        {
            var attachmentContentStorage = ServiceLocator.Current.GetInstance<IAttachmentContentStorage>();
            var attachment = questionnaire.GetAttachmentById(attachmentId.Value);

            var attachmentContentMetadata = attachmentContentStorage.GetMetadata(attachment.ContentId);
            if (!attachmentContentMetadata.ContentType.StartsWith("application/pdf", StringComparison.OrdinalIgnoreCase)) return;

            var pdfService = ServiceLocator.Current.GetInstance<IInterviewPdfService>();
            pdfService.OpenAttachment(sourceEntity.InterviewId, attachmentId.Value);
        }

        private class NavigateToEntitySpan : ClickableSpan
        {
            private readonly Action<string, IInterviewEntity> onClick;
            private readonly string variable;
            private readonly IInterviewEntity interviewEntity;

            public NavigateToEntitySpan(Action<string, IInterviewEntity> onClick, string variable, IInterviewEntity interviewEntity)
            {
                this.onClick = onClick;
                this.variable = variable;
                this.interviewEntity = interviewEntity;
            }

            public override void OnClick(View widget) => onClick.Invoke(variable, interviewEntity);
        }
    }
}
