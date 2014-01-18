using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Capi.Views.InterviewDetails
{
    public class QuestionnairePropagatedScreenViewModel : QuestionnaireScreenViewModel
    {
        public QuestionnairePropagatedScreenViewModel(Guid questionnaireId, string screenName, string title,
            bool enabled,
            InterviewItemId screenId,
            IList<IQuestionnaireItemViewModel> items,
            Func<InterviewItemId, IEnumerable<InterviewItemId>> getSiblings,
            IEnumerable<InterviewItemId> breadcrumbs)
            : this(questionnaireId, screenName, title, enabled, screenId, items, getSiblings, breadcrumbs, null, null, null) { }

        protected QuestionnairePropagatedScreenViewModel(Guid questionnaireId, string screenName, string title,
            bool enabled,
            InterviewItemId screenId,
            IList<IQuestionnaireItemViewModel> items,
            Func<InterviewItemId, IEnumerable<InterviewItemId>> sibligs,
            IEnumerable<InterviewItemId> breadcrumbs, IQuestionnaireItemViewModel next, IQuestionnaireItemViewModel previous,
            int? sortIndex)
            : base(questionnaireId, screenName, title, enabled, screenId, items, breadcrumbs)
        {
            this.sibligsValue = sibligs;
            this.SortIndex = sortIndex;
            if (next != null)
            {
                Next = next;
                Next.PropertyChanged += item_PropertyChanged;
            }
            if (previous != null)
            {
                Previous = previous;
                Previous.PropertyChanged += item_PropertyChanged;
            }
        }

        public QuestionnairePropagatedScreenViewModel Clone(decimal[] propagationVector, int? sortIndex)
        {
            IList<IQuestionnaireItemViewModel> items = this.Items.Select(questionnaireItemViewModel => questionnaireItemViewModel.Clone(propagationVector)).ToList();

            if (!this.ScreenId.IsTopLevel())
                throw new InvalidOperationException("only template can mutate in that way");

            var key = new InterviewItemId(this.ScreenId.Id, propagationVector);
            var breadcrumbs = this.Breadcrumbs.ToArray();

            var lastVector = new decimal[propagationVector.Length];
            propagationVector.CopyTo(lastVector, 0);

            for (int i = breadcrumbs.Length - 1; i >= 0; i--)
            {
                if (EmptyBreadcrumbForRosterRow.IsInterviewItemIdEmptyBreadcrumbForRosterRow(breadcrumbs[i]))
                {
                    breadcrumbs[i] = new InterviewItemId(breadcrumbs[i].Id, lastVector);
                    
                    if (lastVector.Length > 0)
                        lastVector = lastVector.Take(lastVector.Length - 1).ToArray();
                }
            }

            if (lastVector.Length < propagationVector.Length)
                breadcrumbs = breadcrumbs.Take(breadcrumbs.Length - 1).ToArray();

            return new QuestionnairePropagatedScreenViewModel(
                QuestionnaireId,
                ScreenName,
                Title,
                true,
                key,
                items,
                sibligsValue,
                breadcrumbs,
                Next != null ? Next.Clone(propagationVector) : null,
                Previous != null ? Previous.Clone(propagationVector) : null,
                sortIndex);
        }

        public void AddNextPrevious(IQuestionnaireItemViewModel next, IQuestionnaireItemViewModel previous)
        {
            if (!this.ScreenId.IsTopLevel())
                throw new InvalidOperationException("only template can mutate in that way");
            this.Next = next;
            this.Previous = previous;
        }

        private readonly Func<InterviewItemId, IEnumerable<InterviewItemId>> sibligsValue;

        public void UpdateScreenName(string screenName)
        {
            this.ScreenName = screenName;
            RaisePropertyChanged("ScreenName");
        }
        public override IEnumerable<InterviewItemId> Siblings
        {
            get { return this.sibligsValue(this.ScreenId); }
        }

        public IQuestionnaireItemViewModel Next { get; private set; }
        public IQuestionnaireItemViewModel Previous { get; private set; }
        public int? SortIndex { get; private set; }
    }
}