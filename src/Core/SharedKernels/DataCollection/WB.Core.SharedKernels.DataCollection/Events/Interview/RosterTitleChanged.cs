namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    using System;
    using Base;

    public class RosterTitleChanged : QuestionActiveEvent
    {
        public string RosterTitle { set; get; }

        public RosterTitleChanged(Guid userId, Guid questionId, decimal[] propagationVector, string rosterTitle)
            : base(userId, questionId, propagationVector)
        {
            this.RosterTitle = rosterTitle;
        }
    }
}