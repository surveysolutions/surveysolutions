using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.WebApi
{
    public class IdentityApiView
    {
        public Guid QuestionId { get; set; }
        public List<decimal> RosterVector { get; set; } 
    }
}