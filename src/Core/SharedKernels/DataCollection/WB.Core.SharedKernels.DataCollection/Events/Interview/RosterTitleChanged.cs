namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    using System;
    using Base;

    public class RosterTitleChanged : QuestionPassiveEvent
    {
        public string RosterTitle { set; get; }

        public RosterTitleChanged(Guid questionId, decimal[] propagationVector, string rosterTitle)
            : base(questionId, propagationVector)
        {
            this.RosterTitle = rosterTitle;
        }
    }
}