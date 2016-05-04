using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.WebApi
{
    public class RosterInstanceApiView
    {
        public Guid RosterId { get; set; }
        public List<decimal> OuterScopeRosterVector { get; set; }
        public decimal RosterInstanceId { get; set; }
        public int? SortIndex { get; set; }
        public string RosterTitle { get; set; }
    }
}