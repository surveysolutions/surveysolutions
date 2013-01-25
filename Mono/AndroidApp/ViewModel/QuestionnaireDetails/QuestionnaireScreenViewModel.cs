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

        private readonly Func<IEnumerable<QuestionnaireScreenViewModel>> chaptersValue;
        private readonly Func<IEnumerable<QuestionnaireNavigationPanelItem>> breadcrumbsValue;
        private readonly Func<IEnumerable<QuestionnaireNavigationPanelItem>> sibligsValue;
        private readonly Func<string> titleValue;

        public QuestionnaireScreenViewModel(Guid questionnaireId, string screenName, Func<string> title,
                                            ItemPublicKey screenId, IEnumerable<IQuestionnaireItemViewModel> items,
                                            Func<IEnumerable<QuestionnaireNavigationPanelItem>> siblings,
                                            IEnumerable<QuestionnaireNavigationPanelItem> breadcrumbs,
                                            Func<IEnumerable<QuestionnaireScreenViewModel>> chapters)
        {

            QuestionnaireId = questionnaireId;
            Items = items;
            ScreenId = screenId;
            sibligsValue = siblings;
            breadcrumbsValue =
                () =>
                breadcrumbs.Union(new QuestionnaireNavigationPanelItem[1]
                    {new QuestionnaireNavigationPanelItem(ScreenId, Title, 0, 0, true)});
            chaptersValue = chapters;
            ScreenName = screenName;
            titleValue = title;
        }

        public QuestionnaireScreenViewModel(Guid questionnaireId, string screenName, string title,
                                            ItemPublicKey screenId, IEnumerable<IQuestionnaireItemViewModel> items,
                                            IEnumerable<QuestionnaireNavigationPanelItem> siblings,
                                            IEnumerable<QuestionnaireNavigationPanelItem> breadcrumbs,
                                            Func<IEnumerable<QuestionnaireScreenViewModel>> chapters)
        {

            QuestionnaireId = questionnaireId;
            Items = items;
            ScreenId = screenId;
            sibligsValue = () => siblings;
            breadcrumbsValue = () => breadcrumbs;
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
        public int Answered { get; private set; }
        public int Total { get; private set; }
        public ItemPublicKey ScreenId { get; private set; }

        public IEnumerable<QuestionnaireNavigationPanelItem> Siblings
        {
            get { return sibligsValue(); }
        }

        public IEnumerable<QuestionnaireNavigationPanelItem> Breadcrumbs
        {
            get { return breadcrumbsValue(); }
        }

        public IEnumerable<QuestionnaireScreenViewModel> Chapters
        {
            get { return chaptersValue(); }
        }

        public IEnumerable<IQuestionnaireItemViewModel> Items { get; private set; }

    }
}