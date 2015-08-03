using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Tester.Implementation.Entities
{
    public class GroupsHierarchyModel
    {
        public GroupsHierarchyModel()
        {
            this.Children = new List<GroupsHierarchyModel>();
        }

        public Guid Id { get; set; }
        public string Title { get; set; }
        public bool IsRoster { get; set; }
        public int ZeroBasedDepth { get; set; }

        public List<GroupsHierarchyModel> Children { get; set; }
    }
}