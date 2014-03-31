
using System;
using System.Collections.Generic;
using Main.Core.Entities;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.Api.Models
{
    public class UsersListViewModel
    {
        public PagerData Pager { get; set; }

        public Guid? SupervisorId { get; set; }

        public IEnumerable<OrderRequestItem> SortOrder { get; set; }
    }
}