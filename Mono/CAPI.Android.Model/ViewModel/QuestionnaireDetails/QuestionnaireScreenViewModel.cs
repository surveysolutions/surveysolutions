using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails
{
    public class QuestionnaireScreenViewModel : Cirrious.MvvmCross.ViewModels.MvxViewModel, IQuestionnaireViewModel
    {
        protected QuestionnaireScreenViewModel(Guid questionnaireId, string screenName, string title, bool enabled,
                                            InterviewItemId screenId, IList<IQuestionnaireItemViewModel> items,
                                            IEnumerable<InterviewItemId> breadcrumbs, int total, int answered)
        {

            QuestionnaireId = questionnaireId;
            Items = items;
            ScreenId = screenId;
            if (breadcrumbs == null)
                breadcrumbs = new List<InterviewItemId>();
            Breadcrumbs = breadcrumbs.Union(new InterviewItemId[1] { this.ScreenId });
            Title = title;
            Enabled = enabled;
            ScreenName = screenName;
            Total = total;
            Answered = answered;
            bool needCounts = total == 0 && answered == 0;
            foreach (var item in Items)
            {
                item.PropertyChanged += item_PropertyChanged;
                if(needCounts)
                {
                    var question = item as QuestionViewModel;
                    if (question != null)
                    {
                        if (question.IsEnabled())
                        {
                            Total++;
                            if (question.Status.HasFlag(QuestionStatus.Answered))
                                Answered++;
                        }
                        continue;
                    }
                    var group = item as QuestionnaireNavigationPanelItem;
                    if (group != null)
                    {
                        Total += group.Total;
                        Answered = Answered + group.Answered;
                    }
                }
            }
            if(needCounts)
            {
                this.RaisePropertyChanged("Total");
                this.RaisePropertyChanged("Answered");
            }
        }

        public QuestionnaireScreenViewModel(Guid questionnaireId, string screenName, string title, bool enabled,
                                           InterviewItemId screenId, IList<IQuestionnaireItemViewModel> items,
                                           IEnumerable<InterviewItemId> siblings,
                                           IEnumerable<InterviewItemId> breadcrumbs)
            : this(questionnaireId,screenName, title, enabled, screenId, items,siblings, breadcrumbs,0,0)
        {
        }
        [JsonConstructor]
        public QuestionnaireScreenViewModel(Guid questionnaireId, string screenName, string title, bool enabled,
                                          InterviewItemId screenId, IList<IQuestionnaireItemViewModel> items,
                                          IEnumerable<InterviewItemId> siblings,
                                          IEnumerable<InterviewItemId> breadcrumbs, int total, int answered)
            : this(questionnaireId, screenName, title, enabled, screenId, items, breadcrumbs, total, answered)
        {
            Siblings = siblings;
        }
        public Guid QuestionnaireId { get; private set; }
        public InterviewItemId ScreenId { get; private set; }
        public string Title { get; private set; }
        public string ScreenName{get; protected set;}
        public int Answered {
            get { return Enabled ? answered : 0; }
            private set { answered = value; }
        }
        private int answered;

        public int Total
        {
            get { return Enabled ? total : 0; }
            private set { total = value; }
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
            foreach (var item in Items)
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
            if (total != Total)
            {
                Total = total;
                this.RaisePropertyChanged("Total");
            }
            if (answered != Answered)
            {
                Answered = answered;
                this.RaisePropertyChanged("Answered");
            }
        }
       
        public void SetEnabled(bool enabled)
        {
            if (Enabled == enabled)
                return;
            Enabled = enabled;
            this.RaisePropertyChanged("Enabled");
        }

        void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var question = sender as QuestionViewModel;
            if (question != null)
            {
                if (e.PropertyName != "Status")
                    return;
                UpdateCounters();
            }
            var group = sender as QuestionnaireNavigationPanelItem;
            if (group != null)
            {
                if (e.PropertyName != "Answered" && e.PropertyName != "Total")
                    return;
                UpdateCounters();
            }
        }

    }
}