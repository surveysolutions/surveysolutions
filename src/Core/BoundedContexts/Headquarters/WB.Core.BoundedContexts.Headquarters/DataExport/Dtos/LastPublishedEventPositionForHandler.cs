using System;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Dtos
{
    public class LastPublishedEventPositionForHandler : IView
    {
        public LastPublishedEventPositionForHandler()
        {
        }

        public LastPublishedEventPositionForHandler(string eventHandlerName, Guid eventSourceIdOfLastSuccessfullyHandledEvent, int eventSequenceOfLastSuccessfullyHandledEvent, long commitPosition, long preparePosition)
        {
            this.EventHandlerName = eventHandlerName;
            this.EventSourceIdOfLastSuccessfullyHandledEvent = eventSourceIdOfLastSuccessfullyHandledEvent;
            this.EventSequenceOfLastSuccessfullyHandledEvent = eventSequenceOfLastSuccessfullyHandledEvent;
            this.CommitPosition = commitPosition;
            this.PreparePosition = preparePosition;
        }

        public virtual string EventHandlerName { get; set; }
        public virtual Guid EventSourceIdOfLastSuccessfullyHandledEvent { get; set; }
        public virtual int EventSequenceOfLastSuccessfullyHandledEvent { get; set; }
        public virtual long CommitPosition { get;  set; }
        public virtual long PreparePosition { get;  set; }
    }
}