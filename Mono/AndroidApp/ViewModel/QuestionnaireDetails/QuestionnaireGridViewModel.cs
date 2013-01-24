// -----------------------------------------------------------------------
// <copyright file="QuestionnaireGridViewModel.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel;
using AndroidApp.ViewModel.QuestionnaireDetails.GridItems;

namespace AndroidApp.ViewModel.QuestionnaireDetails
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class QuestionnaireGridViewModel : Cirrious.MvvmCross.ViewModels.MvxViewModel,IQuestionnaireViewModel
    {
        #region Implementation of IQuestionnaireViewModel
        private readonly Func<IEnumerable<QuestionnaireNavigationPanelItem>> chaptersValue;
        private readonly Func<IEnumerable<QuestionnaireScreenViewModel>> rowsValue;
        public QuestionnaireGridViewModel(Guid questionnaireId, string screenName,string title, ItemPublicKey screenId, IList<QuestionnaireNavigationPanelItem> siblings,
            IList<QuestionnaireNavigationPanelItem> breadcrumbs, Func<IEnumerable<QuestionnaireNavigationPanelItem>> chapters, IList<HeaderItem> header
            , Func<IEnumerable<QuestionnaireScreenViewModel>> rows)
        {
            QuestionnaireId = questionnaireId;
            ScreenName = screenName;
            Title = title;
            ScreenId = screenId;
            Siblings = siblings;
            Breadcrumbs = breadcrumbs;
            chaptersValue = chapters;
            rowsValue = rows;
            Header = header;
            
        }

        public Guid QuestionnaireId { get; private set; }
        public string Title { get; private set; }
        public string ScreenName { get; private set; }
        public ItemPublicKey ScreenId { get; private set; }
        public IList<QuestionnaireNavigationPanelItem> Siblings { get; private set; }
        public IList<QuestionnaireNavigationPanelItem> Breadcrumbs { get; private set; }
        public IEnumerable<QuestionnaireNavigationPanelItem> Chapters
        {
            get { return chaptersValue(); }
        }
        public IList<HeaderItem> Header { get; private set; }
        public IEnumerable<QuestionnaireScreenViewModel> Rows
        {
            get { return rowsValue(); }
        }

        #endregion

       
    }
}
