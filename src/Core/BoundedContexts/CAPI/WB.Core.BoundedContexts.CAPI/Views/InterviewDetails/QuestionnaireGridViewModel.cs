// -----------------------------------------------------------------------
// <copyright file="QuestionnaireGridViewModel.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails.GridItems;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Capi.Views.InterviewDetails
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class QuestionnaireGridViewModel : Cirrious.MvvmCross.ViewModels.MvxViewModel, IQuestionnaireViewModel
    {
        private Func<IEnumerable<QuestionnairePropagatedScreenViewModel>> rowsValue;
        public QuestionnaireGridViewModel(Guid questionnaireId, string screenName, string title, InterviewItemId screenId, bool enabled, IEnumerable<InterviewItemId> siblings,
            IEnumerable<InterviewItemId> breadcrumbs, IList<HeaderItem> header
            , Func<IEnumerable<QuestionnairePropagatedScreenViewModel>> rows, int total, int answered)
        {
            this.QuestionnaireId = questionnaireId;
            this.ScreenName = screenName;
            this.Title = title;
            this.ScreenId = screenId;
            this.Siblings = siblings;
            if (breadcrumbs == null)
                breadcrumbs = new List<InterviewItemId>();
            this.Breadcrumbs = breadcrumbs.Union(new InterviewItemId[1] { this.ScreenId });
            this.rowsValue = rows;
            this.Header = header;
            this.Enabled = enabled;
            this.Answered = answered;
            this.Total = total;
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
            get { return this.rowsValue(); }
        }
        public IEnumerable<InterviewItemId> Siblings { get; private set; }
        public IEnumerable<InterviewItemId> Breadcrumbs { get; private set; }

        public void SetEnabled(bool enabled)
        {
            if (this.Enabled == enabled)
                return;
            this.Enabled = enabled;
            foreach (var model in this.Rows)
            {
                model.SetEnabled(enabled);
            }
            this.RaisePropertyChanged("Enabled");
        }

        public void RestoreRowFunction(Func<IEnumerable<QuestionnairePropagatedScreenViewModel>> rows)
        {
            this.rowsValue = rows;
        }

        public void UpdateCounters()
        {
            var total = 0;
            var answered = 0;
            foreach (var screenViewModel in this.Rows)
            {
                total = total + screenViewModel.Total;
                answered = answered + screenViewModel.Answered;
            }
            if (total != this.Total)
            {
                this.Total = total;
                this.RaisePropertyChanged("Total");
            }
            if (answered != this.Answered)
            {
                this.Answered = answered;
                this.RaisePropertyChanged("Answered");
            }
        }


    }
}
