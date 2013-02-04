// -----------------------------------------------------------------------
// <copyright file="QuestionnaireGridViewModel.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using AndroidApp.Core.Model.ViewModel.QuestionnaireDetails.GridItems;

namespace AndroidApp.Core.Model.ViewModel.QuestionnaireDetails
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class QuestionnaireGridViewModel : Cirrious.MvvmCross.ViewModels.MvxViewModel, IQuestionnaireViewModel
    {
        #region Implementation of IQuestionnaireViewModel
     
        private  Func<IEnumerable<QuestionnairePropagatedScreenViewModel>> rowsValue;
        public QuestionnaireGridViewModel(Guid questionnaireId, string screenName, string title, ItemPublicKey screenId, bool enabled, IEnumerable<ItemPublicKey> siblings,
            IEnumerable<IQuestionnaireViewModel> breadcrumbs,/* IEnumerable<QuestionnaireScreenViewModel> chapters,*/ IList<HeaderItem> header
            , Func<IEnumerable<QuestionnairePropagatedScreenViewModel>> rows)
        {
            QuestionnaireId = questionnaireId;
            ScreenName = screenName;
            Title = title;
            ScreenId = screenId;
            Siblings = siblings;
            if (breadcrumbs == null)
                breadcrumbs = new List<IQuestionnaireViewModel>();
            Breadcrumbs = breadcrumbs.Union(new IQuestionnaireViewModel[1] {this});
          //  Chapters = chapters;
            rowsValue = rows;
            Header = header;
            Enabled = enabled;
        }

        public Guid QuestionnaireId { get; private set; }
        public string Title { get; private set; }
        public string ScreenName { get; private set; }
        public ItemPublicKey ScreenId { get; private set; }
        public int Total { get; private set; }
        public int Answered { get; private set; }
        public bool Enabled { get; private set; }
        public IList<HeaderItem> Header { get; private set; }
        [JsonIgnore]
        public IEnumerable<QuestionnairePropagatedScreenViewModel> Rows
        {
            get { return rowsValue(); }
        }
        public IEnumerable<ItemPublicKey> Siblings { get; private set; }
        [JsonIgnore]
        public IEnumerable<IQuestionnaireViewModel> Breadcrumbs { get; private set; }
     /*   [JsonIgnore]
        public IEnumerable<QuestionnaireScreenViewModel> Chapters { get; private set; }*/
        
        public void SetEnabled(bool enabled)
        {
            if (Enabled == enabled)
                return;
            Enabled = enabled;
            foreach (var model in Rows)
            {
                model.SetEnabled(enabled);
            }
            RaisePropertyChanged("Enabled");
        }

        #endregion
        public void RestoreRowFunction(Func<IEnumerable<QuestionnairePropagatedScreenViewModel>> rows)
        {
            this.rowsValue = rows;
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

       
    }
}
