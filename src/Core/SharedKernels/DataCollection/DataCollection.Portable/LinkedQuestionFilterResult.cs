using System;

namespace WB.Core.SharedKernels.DataCollection
{
    public class LinkedQuestionFilterResult
    {
        public Guid LinkedQuestionId { get; set; }
        public bool Enabled { get; set; }
        public Identity[] RosterKey { get; set; }
    }
}