using System;
using System.Collections.Generic;
using System.Linq;
using Android.OS;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using CommonMark;
using Java.Lang;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.UI.Shared.Enumerator.ValueCombiners
{
    public class MarkdownTextToHtmlValueCombiner : BaseValueCombiner<ICharSequence>
    {
        protected override int ExpectedParamsCount => 2;

        protected override ICharSequence GetValue(List<object> values)
        {
            string text = values[0]?.ToString() ?? string.Empty;
            var interviewEntity = (IInterviewEntity) values[1];

            if (interviewEntity == null) return new SpannableString(text);

            var htmlText = MarkdownTextToHtml(text);

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

        private static string MarkdownTextToHtml(string title)
        {
            var document = CommonMarkConverter.Parse(title);

            using (var writer = new System.IO.StringWriter())
            {
                CommonMarkConverter.ProcessStage3(document, writer);

                var htmlText = writer.ToString().TrimEnd('\n');
                if (htmlText.StartsWith("<p>") && htmlText.EndsWith("</p>"))
                    htmlText = htmlText.Substring(3, htmlText.Length - 7);

                return htmlText;
            }
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
            entityVariable = entityVariable.ToLower();

            if (entityVariable == "cover")
                sourceEntity.NavigationState.NavigateTo(NavigationIdentity.CreateForCoverScreen());
            else if (entityVariable == "complete")
                sourceEntity.NavigationState.NavigateTo(NavigationIdentity.CreateForCompleteScreen());
            else if (entityVariable == "overview")
                sourceEntity.NavigationState.NavigateTo(NavigationIdentity.CreateForOverviewScreen());
            else
            {
                var interview = ServiceLocator.Current.GetInstance<IStatefulInterviewRepository>()
                .Get(sourceEntity.InterviewId);

                if (interview == null) return;

                var questionnaire = ServiceLocator.Current.GetInstance<IQuestionnaireStorage>()
                    .GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

                if (questionnaire == null) return;

                var questionId = questionnaire.GetQuestionIdByVariable(entityVariable);
                var rosterId = questionnaire.GetRosterIdByVariableName(entityVariable, true);

                if (!questionId.HasValue && !rosterId.HasValue) return;

                var interviewEntities = interview.GetAllIdentitiesForEntityId(questionId ?? rosterId.Value).ToArray();

                var entitiesInTheSameOrDeeperRoster = interviewEntities.Where(x =>
                    x.RosterVector.Identical(sourceEntity.Identity.RosterVector,
                        sourceEntity.Identity.RosterVector.Length)).ToArray();

                var entitiesForSelection = entitiesInTheSameOrDeeperRoster.Any()
                    ? entitiesInTheSameOrDeeperRoster
                    : interviewEntities;

                var nearestInterviewEntity = entitiesForSelection.OrderBy(x => x.RosterVector).FirstOrDefault();
                if (nearestInterviewEntity == null) return;

                if (questionId.HasValue)
                {
                    sourceEntity.NavigationState.NavigateTo(new NavigationIdentity
                    {
                        TargetScreen = questionnaire.IsPrefilled(questionId.Value)
                                ? ScreenType.Identifying
                                : ScreenType.Group,
                        TargetGroup = interview.GetParentGroup(nearestInterviewEntity),
                        AnchoredElementIdentity = nearestInterviewEntity
                    });
                }
                else if (rosterId.HasValue)
                {
                    sourceEntity.NavigationState.NavigateTo(new NavigationIdentity
                    {
                        TargetScreen = ScreenType.Group,
                        TargetGroup = interview.GetParentGroup(nearestInterviewEntity),
                        AnchoredElementIdentity = nearestInterviewEntity
                    });
                }
            }
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
