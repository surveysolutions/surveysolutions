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
    public class MarkdownTitleValueCombiner : BaseValueCombiner<ICharSequence>
    {
        protected override int ExpectedParamsCount => 2;

        protected override ICharSequence GetValue(List<object> values)
        {
            string title = values[0]?.ToString() ?? string.Empty;
            var interviewEntity = (IInterviewEntity)values[1];

            if (interviewEntity == null) return new SpannableString(title);

            var document = CommonMarkConverter.Parse(title);

            using (var writer = new System.IO.StringWriter())
            {
                CommonMarkConverter.ProcessStage3(document, writer);

                ICharSequence sequence = Build.VERSION.SdkInt >= BuildVersionCodes.N
                    ? Html.FromHtml(writer.ToString(), FromHtmlOptions.ModeLegacy)
                    : Html.FromHtml(writer.ToString());

                var strBuilder = new SpannableStringBuilder(sequence);
                var urlSpans = strBuilder.GetSpans(0, sequence.Length(), Class.FromType(typeof(URLSpan)));

                foreach (URLSpan span in urlSpans)
                {
                    if (!Uri.IsWellFormedUriString(span.URL, UriKind.Absolute))
                        this.MakeNavigationLink(strBuilder, span, interviewEntity);
                }

                return strBuilder;
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
            var interview = ServiceLocator.Current.GetInstance<IStatefulInterviewRepository>()
                .Get(sourceEntity.InterviewId);

            if(interview == null) return;

            var questionnaire = ServiceLocator.Current.GetInstance<IQuestionnaireStorage>()
                .GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            if (questionnaire == null) return;

            var questionId = questionnaire.GetQuestionIdByVariable(entityVariable);
            var rosterId = questionnaire.GetRosterIdByVariableName(entityVariable, true);

            if (questionId.HasValue)
            {
                var interviewQuestion = interview.GetAllIdentitiesForEntityId(questionId.Value).FirstOrDefault();

                sourceEntity.NavigationState.NavigateTo(new NavigationIdentity
                {
                    TargetScreen = questionnaire.IsPrefilled(questionId.Value) ? ScreenType.Identifying : ScreenType.Group,
                    TargetGroup = interview.GetParentGroup(interviewQuestion),
                    AnchoredElementIdentity = interviewQuestion
                });
            }
            else if (rosterId.HasValue)
            {
                var interviewRoster = interview.GetAllIdentitiesForEntityId(rosterId.Value)
                    .FirstOrDefault();

                sourceEntity.NavigationState.NavigateTo(new NavigationIdentity
                {
                    TargetScreen = ScreenType.Group,
                    TargetGroup = interview.GetParentGroup(interviewRoster),
                    AnchoredElementIdentity = interviewRoster
                });
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
