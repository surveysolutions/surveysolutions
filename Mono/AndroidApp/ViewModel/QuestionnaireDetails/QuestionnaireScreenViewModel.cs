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

        private readonly Func<IEnumerable<QuestionnaireNavigationPanelItem>> chaptersValue;
        private readonly Func<IEnumerable<QuestionnaireNavigationPanelItem>> sibligsValue;
        private readonly Func<string> titleValue;
        public QuestionnaireScreenViewModel(Guid questionnaireId, string screenName, Func<string> title,
            ItemPublicKey screenId, IEnumerable<IQuestionnaireItemViewModel> items,
            Func<IEnumerable<QuestionnaireNavigationPanelItem>> siblings,
            IList<QuestionnaireNavigationPanelItem> breadcrumbs, Func<IEnumerable<QuestionnaireNavigationPanelItem>> chapters)
        {

            QuestionnaireId = questionnaireId;
            Items = items;
            ScreenId = screenId;
            sibligsValue = siblings;
            Breadcrumbs = breadcrumbs;
            chaptersValue = chapters;
            ScreenName = screenName;
            titleValue = title;
        }
        public QuestionnaireScreenViewModel(Guid questionnaireId, string screenName, string title,
            ItemPublicKey screenId, IEnumerable<IQuestionnaireItemViewModel> items,
            IEnumerable<QuestionnaireNavigationPanelItem> siblings,
            IList<QuestionnaireNavigationPanelItem> breadcrumbs, Func<IEnumerable<QuestionnaireNavigationPanelItem>> chapters)
        {

            QuestionnaireId = questionnaireId;
            Items = items;
            ScreenId = screenId;
            sibligsValue = () => siblings;
            Breadcrumbs = breadcrumbs;
            chaptersValue = chapters;
            ScreenName = screenName;
            titleValue = () => title;
        }

        public Guid QuestionnaireId { get; private set; }
        public string Title
        {
            get { return titleValue(); }
        }
        public string ScreenName { get; private set; }
        public ItemPublicKey ScreenId { get; private set; }
        public IEnumerable<QuestionnaireNavigationPanelItem> Siblings
        {
            get { return sibligsValue(); }
        }
        public IList<QuestionnaireNavigationPanelItem> Breadcrumbs { get; private set; }
        public IEnumerable<QuestionnaireNavigationPanelItem> Chapters { get { return chaptersValue(); }
    }

        public IEnumerable<IQuestionnaireItemViewModel> Items { get; private set; }

    }
}