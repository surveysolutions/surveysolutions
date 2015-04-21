using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Entities
{
    public class GroupsHierarchyModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public bool IsRoster { get; set; }
        public List<GroupsHierarchyModel> Children { get; set; }
    }
}