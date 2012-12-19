// -----------------------------------------------------------------------
// <copyright file="QuestionnaireGridViewModel.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

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
    public class QuestionnaireGridViewModel : IQuestionnaireViewModel
    {
        #region Implementation of IQuestionnaireViewModel

        public QuestionnaireGridViewModel(Guid questionnaireId, string screenName, ItemPublicKey screenId, IList<QuestionnaireNavigationPanelItem> siblings,
            IEnumerable<QuestionnaireNavigationPanelItem> breadcrumbs, IEnumerable<QuestionnaireNavigationPanelItem> chapters, IEnumerable<HeaderItem> header,
            IEnumerable<RosterItem> rows)
        {
            QuestionnaireId = questionnaireId;
            ScreenName = screenName;
            ScreenId = screenId;
            Siblings = siblings;
            Breadcrumbs = breadcrumbs;
            Chapters = chapters;
            Header = header;
            Rows = rows;
        }

        public Guid QuestionnaireId { get; private set; }
        public string ScreenName { get; private set; }
        public ItemPublicKey ScreenId { get; private set; }
        public IList<QuestionnaireNavigationPanelItem> Siblings { get; private set; }
        public IEnumerable<QuestionnaireNavigationPanelItem> Breadcrumbs { get; private set; }
        public IEnumerable<QuestionnaireNavigationPanelItem> Chapters { get; private set; }
        public IEnumerable<HeaderItem> Header { get; private set; }
        public IEnumerable<RosterItem> Rows { get; private set; }

        #endregion
    }
}
