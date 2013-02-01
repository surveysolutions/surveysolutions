using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;

namespace AndroidApp.Core.Model.ViewModel.QuestionnaireDetails
{
    public class QuestionnaireScreenViewModel : Cirrious.MvvmCross.ViewModels.MvxViewModel, IQuestionnaireViewModel
    {
        protected QuestionnaireScreenViewModel(Guid questionnaireId, string screenName, string title, bool enabled,
                                            ItemPublicKey screenId, IEnumerable<IQuestionnaireItemViewModel> items,
                                            IEnumerable<IQuestionnaireViewModel> breadcrumbs,
                                            IEnumerable<QuestionnaireScreenViewModel> chapters)
        {

            QuestionnaireId = questionnaireId;
            Items = items;
            ScreenId = screenId;
            if (breadcrumbs != null)
                Breadcrumbs = breadcrumbs.Union(new IQuestionnaireViewModel[1] {this});
            Chapters = chapters;
            Title = title;
            Enabled = enabled;
            ScreenName = screenName;
            foreach (var item in Items)
            {
                item.PropertyChanged += item_PropertyChanged;

            }
        }

        public QuestionnaireScreenViewModel(Guid questionnaireId, string screenName, string title, bool enabled,
                                           ItemPublicKey screenId, IEnumerable<IQuestionnaireItemViewModel> items,
                                           IEnumerable<ItemPublicKey> siblings,
                                           IEnumerable<IQuestionnaireViewModel> breadcrumbs,
                                           IEnumerable<QuestionnaireScreenViewModel> chapters)
            : this(questionnaireId,screenName, title, enabled, screenId, items, breadcrumbs, chapters)
        {
            Siblings = siblings;
        }

        public Guid QuestionnaireId { get; private set; }
        public ItemPublicKey ScreenId { get; private set; }
        public string Title { get; private set; }
        public string ScreenName{get; protected set;}
        public int Answered { get; private set; }
        public int Total { get; private set; }
        public bool Enabled { get; private set; }
        public IEnumerable<IQuestionnaireItemViewModel> Items { get; private set; }
        public virtual IEnumerable<ItemPublicKey> Siblings { get; private set; }

        [JsonIgnore]
        public IEnumerable<IQuestionnaireViewModel> Breadcrumbs { get; protected set; }
        [JsonIgnore]
        public IEnumerable<QuestionnaireScreenViewModel> Chapters { get; private set; }
        

        protected void UpdateCounters()
        {
            var total = 0;
            var answered = 0;
            foreach (var item in Items)
            {
                var question = item as QuestionViewModel;
                if (question != null)
                {
                    if (question.Status.HasFlag(QuestionStatus.Enabled))
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
            RaisePropertyChanged("Enabled");
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