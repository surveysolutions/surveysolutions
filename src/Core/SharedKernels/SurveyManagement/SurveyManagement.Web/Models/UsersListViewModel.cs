// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UsersListViewModel.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Main.Core.Entities;
using WB.Core.GenericSubdomains.Utils;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class UsersListViewModel : IGridRequest<UsersRequestModel>
    {
        #region Public Properties

        public PagerData Pager { get; set; }

        public UsersRequestModel Request { get; set; }

        public IEnumerable<OrderRequestItem> SortOrder { get; set; }

        #endregion
    }
}