using System.Collections.Generic;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interviewer
{
    public class InterviewersView : IListView<InterviewersItem>
    {
        #region Public Properties
        
        /// <summary>
        /// Gets or sets the total count.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets Items.
        /// </summary>
        public IEnumerable<InterviewersItem> Items { get; set; }

        public InterviewersItem ItemsSummary { get; set; }

        #endregion
    }
}