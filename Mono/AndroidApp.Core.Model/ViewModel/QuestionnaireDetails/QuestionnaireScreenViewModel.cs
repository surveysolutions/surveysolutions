using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace AndroidApp.Core.Model.ViewModel.QuestionnaireDetails
{
    public class QuestionnaireScreenViewModel : Cirrious.MvvmCross.ViewModels.MvxViewModel, IQuestionnaireViewModel
    {

        private readonly Func<IEnumerable<QuestionnaireScreenViewModel>> chaptersValue;
        private readonly Func<IEnumerable<ItemPublicKey>> sibligsValue;
        public QuestionnaireScreenViewModel(Guid questionnaireId, string title, bool enabled,
                                           ItemPublicKey screenId, IEnumerable<IQuestionnaireItemViewModel> items,
                                           Func<IEnumerable<ItemPublicKey>> siblings,
                                           IEnumerable<IQuestionnaireViewModel> breadcrumbs,
                                           Func<IEnumerable<QuestionnaireScreenViewModel>> chapters, bool updateBreadBrumbs)
        {

            QuestionnaireId = questionnaireId;
            Items = items;
            ScreenId = screenId;
            sibligsValue = siblings;
            Breadcrumbs = updateBreadBrumbs ? breadcrumbs.Union(new IQuestionnaireViewModel[1] { this }) : breadcrumbs.ToList();
            chaptersValue = chapters;
            Title = title;
            Enabled = enabled;

            foreach (var item in Items)
            {
                item.PropertyChanged += item_PropertyChanged;

            }

        }

        public QuestionnaireScreenViewModel(Guid questionnaireId, string screenName, string title, bool enabled,
                                           ItemPublicKey screenId, IEnumerable<IQuestionnaireItemViewModel> items,
                                           IEnumerable<ItemPublicKey> siblings,
                                           IEnumerable<IQuestionnaireViewModel> breadcrumbs,
                                           Func<IEnumerable<QuestionnaireScreenViewModel>> chapters, bool updateBreadBrumbs)
            : this(questionnaireId,  title, enabled, screenId, items, () => siblings, breadcrumbs, chapters, updateBreadBrumbs)
        {
            this.ScreenName = screenName;
        }

        public string ScreenName
        {
            get; private set; //   get { return screenNameValue(); }
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
                if (e.PropertyName != "Answered" &&  e.PropertyName != "Total")
                    return;
                UpdateCounters();
            }
        }
        public void UpdateScreenName(string screenName)
        {
            this.ScreenName = screenName;
            RaisePropertyChanged("ScreenName");
        }


        /*
        void question_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

            if (e.PropertyName != "Status")
                return;
            var question = sender as QuestionViewModel;
            if (question == null)
                return;
            var screen =
                this.Screens.Select(s => s.Value).OfType<QuestionnaireScreenViewModel>().FirstOrDefault(s => s.Items.Any(i => i.PublicKey == question.PublicKey));
            if (screen == null)
                return;
            var breadcrumbs = screen.Breadcrumbs.ToList();
            for (int i = breadcrumbs.Count - 1; i >= 0; i--)
            {
                breadcrumbs[i].UpdateCounters();
            }
        }*/
        public Guid QuestionnaireId { get; private set; }

        public string Title { get; private set; }

       
        public int Answered { get; private set; }
        public int Total { get; private set; }
        public ItemPublicKey ScreenId { get; private set; }

        public IEnumerable<ItemPublicKey> Siblings
        {
            get { return sibligsValue(); }
        }

        public IEnumerable<IQuestionnaireViewModel> Breadcrumbs { get; private set; }

        public IEnumerable<QuestionnaireScreenViewModel> Chapters
        {
            get { return chaptersValue(); }
        }

        public IEnumerable<IQuestionnaireItemViewModel> Items { get; private set; }
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

        public bool Enabled { get; private set; }
        public void SetEnabled(bool enabled)
        {
            if (Enabled == enabled)
                return;
            Enabled = enabled;
            RaisePropertyChanged("Enabled");
        }
    }
}