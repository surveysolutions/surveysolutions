using System;
using System.Collections.Generic;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Java.Lang;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.UI.Shared.Enumerator.ValueCombiners
{
    public class MakeMarkdownLinksExecutableValueCombiner : BaseValueCombiner<ICharSequence>
    {
        private readonly EntityNavigator navigator;

        public MakeMarkdownLinksExecutableValueCombiner() : this(new EntityNavigator())
        {
        }

        public MakeMarkdownLinksExecutableValueCombiner(EntityNavigator navigator)
        {
            this.navigator = navigator;
        }

        protected override int ExpectedParamsCount => 2;

        protected override ICharSequence GetValue(List<object> values)
        {
            string htmlText = values[0]?.ToString() ?? string.Empty;
            var interviewEntity = (IInterviewEntity) values[1];

            if (interviewEntity == null) return new SpannableString(htmlText);

#pragma warning disable CA1416 // Validate platform compatibility
            ICharSequence sequence = Html.FromHtml(htmlText, FromHtmlOptions.ModeLegacy);
#pragma warning restore CA1416 // Validate platform compatibility

            if (interviewEntity.InterviewId == null) 
                return new SpannableString(sequence);

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

            var navigationSpan = new NavigateToEntitySpan(this.navigator, span.URL, interviewEntity);

            strBuilder.SetSpan(navigationSpan, start, end, flags);
            strBuilder.RemoveSpan(span);
        }

        private class NavigateToEntitySpan : ClickableSpan
        {
            private EntityNavigator navigator;
            private string variable;
            private IInterviewEntity interviewEntity;

            public NavigateToEntitySpan(EntityNavigator navigator, string variable, IInterviewEntity interviewEntity)
            {
                this.navigator = navigator;
                this.variable = variable;
                this.interviewEntity = interviewEntity;
            }

            public override void OnClick(View widget) => _ = navigator.NavigateToEntity(variable, interviewEntity);

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    this.navigator = null;
                    this.variable = null;
                    this.interviewEntity = null;
                }
                
                base.Dispose(disposing);
            }
        }
    }
}
