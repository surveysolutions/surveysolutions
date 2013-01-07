using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace AndroidApp.ViewModel.QuestionnaireDetails
{
    public class QuestionnaireScreenViewModel : IQuestionnaireViewModel
    {
        public QuestionnaireScreenViewModel(Guid questionnaireId,string screenName,string title,
            ItemPublicKey screenId, IEnumerable<IQuestionnaireItemViewModel> items,
            IList<QuestionnaireNavigationPanelItem> siblings,
            IEnumerable<QuestionnaireNavigationPanelItem> breadcrumbs, IEnumerable<QuestionnaireNavigationPanelItem> chapters)
        {

            QuestionnaireId = questionnaireId;
            Items = items;
            ScreenId = screenId;
            Siblings = siblings;
            Breadcrumbs = breadcrumbs;
            Chapters = chapters;
            ScreenName = screenName;
            Title = title;
        }

        public Guid QuestionnaireId { get; private set; }
        public string Title { get; private set; }
        public string ScreenName { get; private set; }
        public ItemPublicKey ScreenId { get; private set; }
        public IList<QuestionnaireNavigationPanelItem> Siblings { get; private set; }
        public IEnumerable<QuestionnaireNavigationPanelItem> Breadcrumbs { get; private set; }
        public IEnumerable<QuestionnaireNavigationPanelItem> Chapters { get; private set; }

        public IEnumerable<IQuestionnaireItemViewModel> Items { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}