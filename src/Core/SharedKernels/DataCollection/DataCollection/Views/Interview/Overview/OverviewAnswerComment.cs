using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.DataCollection.Views.Interview.Overview
{
    public class OverviewAnswerComment 
    {
        public string Text { get; set; }
        public bool IsOwnComment { get; set; }
        public UserRoles UserRole { get; set; }
        public DateTime CommentTimeUtc { get; set; }
        public string EntityId { get; set; }
    }
}
