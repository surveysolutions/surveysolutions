// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UsersListViewModel.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Web.Supervisor.Models
{
    using System.Collections.Generic;

    using Main.Core.Entities;

    public class UsersListViewModel : IGridRequest<UsersRequestModel>
    {
        #region Public Properties

        public PagerData Pager { get; set; }

        public UsersRequestModel Request { get; set; }

        public IEnumerable<OrderRequestItem> SortOrder { get; set; }

        #endregion
    }
}