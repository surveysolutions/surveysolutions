// -----------------------------------------------------------------------
// <copyright file="QuestionnaireGridViewModel.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails.GridItems;
using Newtonsoft.Json;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class QuestionnaireGridViewModel : Cirrious.MvvmCross.ViewModels.MvxViewModel, IQuestionnaireViewModel
    {
        private  Func<IEnumerable<QuestionnairePropagatedScreenViewModel>> rowsValue;
        public QuestionnaireGridViewModel(Guid questionnaireId, string screenName, string title, InterviewItemId screenId, bool enabled, IEnumerable<InterviewItemId> siblings,
            IEnumerable<InterviewItemId> breadcrumbs, IList<HeaderItem> header
            , Func<IEnumerable<QuestionnairePropagatedScreenViewModel>> rows, int total, int answered)
        {
            QuestionnaireId = questionnaireId;
            ScreenName = screenName;
            Title = title;
            ScreenId = screenId;
            Siblings = siblings;
            if (breadcrumbs == null)
                breadcrumbs = new List<InterviewItemId>();
            Breadcrumbs = breadcrumbs.Union(new InterviewItemId[1] { this.ScreenId });
            rowsValue = rows;
            Header = header;
            Enabled = enabled;
            Answered = answered;
            Total = total;
        }
        [JsonConstructor]
        public QuestionnaireGridViewModel(Guid questionnaireId, string screenName, string title, InterviewItemId screenId, bool enabled, IEnumerable<InterviewItemId> siblings,
           IEnumerable<InterviewItemId> breadcrumbs, IList<HeaderItem> header
           , Func<IEnumerable<QuestionnairePropagatedScreenViewModel>> rows)
            : this(questionnaireId, screenName, title, screenId, enabled, siblings, breadcrumbs, header, rows, 0, 0)
        {
        }

        public Guid QuestionnaireId { get; private set; }
        public string Title { get; private set; }
        public string ScreenName { get; private set; }
        public InterviewItemId ScreenId { get; private set; }
        public int Total { get; private set; }
        public int Answered { get; private set; }
        public bool Enabled { get; private set; }
        public IList<HeaderItem> Header { get; private set; }
        [JsonIgnore]
        public IEnumerable<QuestionnairePropagatedScreenViewModel> Rows
        {
            get { return rowsValue(); }
        }
        public IEnumerable<InterviewItemId> Siblings { get; private set; }
        public IEnumerable<InterviewItemId> Breadcrumbs { get; private set; }
        
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
