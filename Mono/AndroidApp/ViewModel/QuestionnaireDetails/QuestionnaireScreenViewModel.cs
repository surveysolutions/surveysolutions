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
        private readonly Func<IEnumerable<ItemPublicKey>> sibligsValue;
        private readonly Func<string> screenNameValue;

        public QuestionnaireScreenViewModel(Guid questionnaireId, Func<string> screenName, string title,
                                            ItemPublicKey screenId, IEnumerable<IQuestionnaireItemViewModel> items,
                                            Func<IEnumerable<ItemPublicKey>> siblings,
                                            IEnumerable<IQuestionnaireViewModel> breadcrumbs,
                                            Func<IEnumerable<QuestionnaireScreenViewModel>> chapters)
        {

            QuestionnaireId = questionnaireId;
            Items = items;
            ScreenId = screenId;
            sibligsValue = siblings;
            Breadcrumbs =
                breadcrumbs.Union(new IQuestionnaireViewModel[1]
                    {this});
            chaptersValue = chapters;
            screenNameValue = screenName;
            Title = title;
        }

        public QuestionnaireScreenViewModel(Guid questionnaireId, string screenName, string title,
                                            ItemPublicKey screenId, IEnumerable<IQuestionnaireItemViewModel> items,
                                            IEnumerable<ItemPublicKey> siblings,
                                            IEnumerable<IQuestionnaireViewModel> breadcrumbs,
                                            Func<IEnumerable<QuestionnaireScreenViewModel>> chapters):this(questionnaireId,()=>screenName,title,screenId,items,()=>siblings,breadcrumbs,chapters)
        {
        }

        public Guid QuestionnaireId { get; private set; }

        public string Title { get; private set; }

        public string ScreenName
        {
            get { return screenNameValue(); }
        }
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

    }
}