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
        private readonly Func<IEnumerable<QuestionnaireScreenViewModel>> chaptersValue;
        private readonly Func<IEnumerable<QuestionnaireScreenViewModel>> rowsValue;
        public QuestionnaireGridViewModel(Guid questionnaireId, string screenName, string title, ItemPublicKey screenId, IEnumerable<ItemPublicKey> siblings,
            IEnumerable<IQuestionnaireViewModel> breadcrumbs, Func<IEnumerable<QuestionnaireScreenViewModel>> chapters, IList<HeaderItem> header
            , Func<IEnumerable<QuestionnaireScreenViewModel>> rows)
        {
            QuestionnaireId = questionnaireId;
            ScreenName = screenName;
            Title = title;
            ScreenId = screenId;
            Siblings = siblings;
            Breadcrumbs = breadcrumbs.Union(new IQuestionnaireViewModel[1] {this});
            chaptersValue = chapters;
            rowsValue = rows;
            Header = header;
            
        }

        public Guid QuestionnaireId { get; private set; }
        public string Title { get; private set; }
        public string ScreenName { get; private set; }
        public ItemPublicKey ScreenId { get; private set; }
        public int Total { get; private set; }
        public int Answered { get; private set; }
        public IEnumerable<ItemPublicKey> Siblings { get; private set; }
        public IEnumerable<IQuestionnaireViewModel> Breadcrumbs { get; private set; }
        public IEnumerable<QuestionnaireScreenViewModel> Chapters
        {
            get { return chaptersValue(); }
        }

        public void UpdateCounters()
        {
            var total = 0;
            var answered = 0;
            foreach (var screenViewModel in Rows)
            {
                total = total + screenViewModel.Total;
                answered = answered + screenViewModel.Answered;
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

        public IList<HeaderItem> Header { get; private set; }
        public IEnumerable<QuestionnaireScreenViewModel> Rows
        {
            get { return rowsValue(); }
        }

        #endregion

       
    }
}
