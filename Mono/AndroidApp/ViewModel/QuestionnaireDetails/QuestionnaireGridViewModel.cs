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

        public QuestionnaireGridViewModel(Guid questionnaireId, string screenName,string title, ItemPublicKey screenId, IList<QuestionnaireNavigationPanelItem> siblings,
            IEnumerable<QuestionnaireNavigationPanelItem> breadcrumbs, IEnumerable<QuestionnaireNavigationPanelItem> chapters, IList<HeaderItem> header,
            IEnumerable<RosterItem> rows)
        {
            QuestionnaireId = questionnaireId;
            ScreenName = screenName;
            Title = title;
            ScreenId = screenId;
            Siblings = siblings;
            Breadcrumbs = breadcrumbs;
            Chapters = chapters;
            Header = header;
            Rows = rows;
        }

        public Guid QuestionnaireId { get; private set; }
        public string Title { get; private set; }
        public string ScreenName { get; private set; }
        public ItemPublicKey ScreenId { get; private set; }
        public IList<QuestionnaireNavigationPanelItem> Siblings { get; private set; }
        public IEnumerable<QuestionnaireNavigationPanelItem> Breadcrumbs { get; private set; }
        public IEnumerable<QuestionnaireNavigationPanelItem> Chapters { get; private set; }
        public IList<HeaderItem> Header { get; private set; }
        public IEnumerable<RosterItem> Rows { get; private set; }

        #endregion

       
    }
}
