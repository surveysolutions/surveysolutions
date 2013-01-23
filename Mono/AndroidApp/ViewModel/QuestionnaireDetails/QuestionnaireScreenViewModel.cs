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
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.Interfaces.Views;
using Java.IO;

namespace AndroidApp.ViewModel.QuestionnaireDetails
{
    public class QuestionnaireScreenViewModel : Cirrious.MvvmCross.ViewModels.MvxViewModel, IQuestionnaireViewModel
    {
        public QuestionnaireScreenViewModel(string completeQuestionnaireId)
        {
            QuestionnaireId = Guid.Parse(completeQuestionnaireId);
        }

        public QuestionnaireScreenViewModel(Guid questionnaireId,string screenName,string title,
            ItemPublicKey screenId, IEnumerable<IQuestionnaireItemViewModel> items,
            IList<QuestionnaireNavigationPanelItem> siblings,
            IList<QuestionnaireNavigationPanelItem> breadcrumbs, IEnumerable<QuestionnaireNavigationPanelItem> chapters)
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
        public IList<QuestionnaireNavigationPanelItem> Breadcrumbs { get; private set; }
        public IEnumerable<QuestionnaireNavigationPanelItem> Chapters { get; private set; }

        public IEnumerable<IQuestionnaireItemViewModel> Items { get; private set; }

    }
}