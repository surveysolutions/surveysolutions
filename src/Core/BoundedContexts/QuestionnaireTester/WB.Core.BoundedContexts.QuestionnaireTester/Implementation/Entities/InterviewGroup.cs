using System;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities
{
    public class InterviewGroup
    {
        public Guid Id { get; set; }
        public decimal[] RosterVector { get; set; }
        public bool IsDisabled { get; set; }
    }
}