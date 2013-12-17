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
                                                      IEnumerable<InterviewItemId> breadcrumbs, int total, int answered,
            IQuestionnaireItemViewModel next, IQuestionnaireItemViewModel previous
            )
            : base(questionnaireId, screenName, title, enabled, screenId, items, breadcrumbs,  total,  answered)
        {
            this.Next = next;
            this.Previous = previous;
        }
        public QuestionnairePropagatedScreenViewModel(Guid questionnaireId, string title,
                                                      bool enabled,
                                                      InterviewItemId screenId,
                                                      IList<IQuestionnaireItemViewModel> items,
                                                      Func<Guid, IEnumerable<InterviewItemId>> sibligs,
                                                      IEnumerable<InterviewItemId> breadcrumbs, int rowIndex)
            : this(questionnaireId, title, enabled, screenId, items, sibligs, breadcrumbs, null, null, rowIndex)
        {
        }
        protected QuestionnairePropagatedScreenViewModel(Guid questionnaireId, string title,
            bool enabled,
            InterviewItemId screenId,
            IList<IQuestionnaireItemViewModel> items,
            Func<Guid, IEnumerable<InterviewItemId>> sibligs,
            IEnumerable<InterviewItemId> breadcrumbs, IQuestionnaireItemViewModel next, IQuestionnaireItemViewModel previous,
            int? sortIndex)
            : this(questionnaireId, title, title, enabled, screenId, items, breadcrumbs, 0, 0, next, previous)
        {
            this.sibligsValue = sibligs;
            if (!screenId.IsTopLevel())
            {

                this.ScreenName = string.Empty;
            }
            this.SortIndex = sortIndex;
        }

        public QuestionnairePropagatedScreenViewModel Clone(decimal[] propagationVector, int? sortIndex)
        {
            IList<IQuestionnaireItemViewModel> items = this.Items.Select(questionnaireItemViewModel => questionnaireItemViewModel.Clone(propagationVector)).ToList();

            if (!this.ScreenId.IsTopLevel())
                throw new InvalidOperationException("only template can mutate in that way");

            var key = new InterviewItemId(this.ScreenId.Id, propagationVector);
            var bradCrumbs = this.Breadcrumbs.ToList();

            return new QuestionnairePropagatedScreenViewModel(this.QuestionnaireId,
                this.Title, true,
                key, items,
                this.sibligsValue, bradCrumbs,
                this.Next != null
                    ? this.Next.Clone(propagationVector)
                    : null,
                this.Previous != null
                    ? this.Previous.Clone(propagationVector)
                    : null,
                sortIndex);
        }

        public void AddNextPrevious(IQuestionnaireItemViewModel next, IQuestionnaireItemViewModel previous)
        {
            if (!this.ScreenId.IsTopLevel())
                throw new InvalidOperationException("only template can mutate in that way");
            this.Next = next;
            this.Previous = previous;
        }

        private readonly Func<Guid, IEnumerable<InterviewItemId>> sibligsValue;

        public void UpdateScreenName(string screenName)
        {
            this.ScreenName = screenName;
            RaisePropertyChanged("ScreenName");
        }
        public override IEnumerable<InterviewItemId> Siblings
        {
            get { return this.sibligsValue(this.ScreenId.Id); }
        }

        public IQuestionnaireItemViewModel Next { get; private set; }
        public IQuestionnaireItemViewModel Previous { get; private set; }
        public int? SortIndex { get; private set; }
    }
}