using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Capi.Views.InterviewDetails
{
    public class QuestionnaireScreenViewModel : Cirrious.MvvmCross.ViewModels.MvxViewModel, IQuestionnaireViewModel
    {
        protected QuestionnaireScreenViewModel(Guid questionnaireId, string screenName, string title, bool enabled,
                                            InterviewItemId screenId, IList<IQuestionnaireItemViewModel> items,
                                            IEnumerable<InterviewItemId> breadcrumbs, int total, int answered)
        {

            this.QuestionnaireId = questionnaireId;
            this.Items = items;
            this.ScreenId = screenId;
            if (breadcrumbs == null)
                breadcrumbs = new List<InterviewItemId>();
            this.Breadcrumbs = breadcrumbs.Union(new InterviewItemId[1] { this.ScreenId });
            this.Title = title;
            this.Enabled = enabled;
            this.ScreenName = screenName;
            this.Total = total;
            this.Answered = answered;
            bool needCounts = total == 0 && answered == 0;
            foreach (var item in this.Items)
            {
                item.PropertyChanged += item_PropertyChanged;
                if (needCounts)
                {
                    var question = item as QuestionViewModel;
                    if (question != null)
                    {
                        if (question.IsEnabled())
                        {
                            this.Total++;
                            if (question.Status.HasFlag(QuestionStatus.Answered))
                                this.Answered++;
                        }
                        continue;
                    }
                    var group = item as QuestionnaireNavigationPanelItem;
                    if (group != null)
                    {
                        this.Total += group.Total;
                        this.Answered = this.Answered + group.Answered;
                    }
                }
            }
            if (needCounts)
            {
                this.RaisePropertyChanged("Total");
                this.RaisePropertyChanged("Answered");
            }
        }

        public QuestionnaireScreenViewModel(Guid questionnaireId, string screenName, string title, bool enabled,
                                           InterviewItemId screenId, IList<IQuestionnaireItemViewModel> items,
                                           IEnumerable<InterviewItemId> siblings,
                                           IEnumerable<InterviewItemId> breadcrumbs)
            : this(questionnaireId, screenName, title, enabled, screenId, items, siblings, breadcrumbs, 0, 0)
        {
        }
        [JsonConstructor]
        public QuestionnaireScreenViewModel(Guid questionnaireId, string screenName, string title, bool enabled,
                                          InterviewItemId screenId, IList<IQuestionnaireItemViewModel> items,
                                          IEnumerable<InterviewItemId> siblings,
                                          IEnumerable<InterviewItemId> breadcrumbs, int total, int answered)
            : this(questionnaireId, screenName, title, enabled, screenId, items, breadcrumbs, total, answered)
        {
            this.Siblings = siblings;
        }
        public Guid QuestionnaireId { get; private set; }
        public InterviewItemId ScreenId { get; private set; }
        public string Title { get; private set; }
        public string ScreenName { get; protected set; }
        public int Answered
        {
            get { return this.Enabled ? this.answered : 0; }
            private set { this.answered = value; }
        }
        private int answered;

        public int Total
        {
            get { return this.Enabled ? this.total : 0; }
            private set { this.total = value; }
        }

        private int total;
        public bool Enabled { get; private set; }
        public IList<IQuestionnaireItemViewModel> Items { get; private set; }

        public virtual IEnumerable<InterviewItemId> Siblings { get; private set; }


        public IEnumerable<InterviewItemId> Breadcrumbs { get; protected set; }
        

        protected void UpdateCounters()
        {
            var total = 0;
            var answered = 0;
            foreach (var item in this.Items)
            {
                var question = item as QuestionViewModel;
                if (question != null)
                {
                    if (question.IsEnabled())
                    {
                        total++;
                        if (question.Status.HasFlag(QuestionStatus.Answered))
                            answered++;
                    }
                    continue;
                }
                var group = item as QuestionnaireNavigationPanelItem;
                if (group != null)
                {
                    total = total + group.Total;
                    answered = answered + group.Answered;
                }

            }
            if (total != this.Total)
            {
                this.Total = total;
                this.RaisePropertyChanged("Total");
            }
            if (answered != this.Answered)
            {
                this.Answered = answered;
                this.RaisePropertyChanged("Answered");
            }
        }

        public void SetEnabled(bool enabled)
        {
            if (this.Enabled == enabled)
                return;
            this.Enabled = enabled;
            this.RaisePropertyChanged("Enabled");
        }

        void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var question = sender as QuestionViewModel;
            if (question != null)
            {
                if (e.PropertyName != "Status")
                    return;
                this.UpdateCounters();
            }
            var group = sender as QuestionnaireNavigationPanelItem;
            if (group != null)
            {
                if (e.PropertyName != "Answered" && e.PropertyName != "Total")
                    return;
                this.UpdateCounters();
            }
        }

    }
}