using System;
using Ncqrs.Commanding;

namespace WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent
{
    public abstract class CalendarEventCommand : CommandBase
    {
        public Guid PublicKey { get; }

        public Guid UserId
        {
            get;
        }

        protected CalendarEventCommand(Guid publicKey, Guid userId)
            : base(publicKey)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty.");
            this.UserId = userId;
            
            this.PublicKey = publicKey;
            this.OriginDate = DateTimeOffset.Now;
        }

        public DateTimeOffset OriginDate { get; }
    }
}
