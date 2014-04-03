using System.Collections.Generic;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Survey
{
    public class SurveyUsersView
    {
        #region Public Properties

        public IEnumerable<UsersViewItem> Items { get; set; }

        #endregion
    }
}