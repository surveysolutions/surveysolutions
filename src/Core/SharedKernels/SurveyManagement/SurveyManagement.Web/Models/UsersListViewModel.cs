using System.Collections.Generic;
using WB.Core.GenericSubdomains.Utils;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class UsersListViewModel : IGridRequest<UsersRequestModel>
    {
        public PagerData Pager { get; set; }

        public UsersRequestModel Request { get; set; }

        public string SearchBy { get; set; }

        public IEnumerable<OrderRequestItem> SortOrder { get; set; }
    }
}