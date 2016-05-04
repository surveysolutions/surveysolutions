using System.Collections.Generic;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Views.User
{
    public class UsersView
    {
        public IEnumerable<UsersViewItem> Users { get; set; }
        public int TotalCountByQuery { get; set; }
    }
}