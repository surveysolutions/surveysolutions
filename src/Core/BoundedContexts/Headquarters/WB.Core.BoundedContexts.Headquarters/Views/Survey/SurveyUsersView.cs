using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

namespace WB.Core.BoundedContexts.Headquarters.Views.Survey
{
    public class SurveyUsersView
    {
        #region Public Properties

        public IEnumerable<UsersViewItem> Items { get; set; }

        #endregion
    }
}