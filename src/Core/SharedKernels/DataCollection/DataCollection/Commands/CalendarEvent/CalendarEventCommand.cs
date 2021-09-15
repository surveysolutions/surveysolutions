using System;
using Ncqrs.Commanding;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent
{
    public abstract class CalendarEventCommand : CommandBase
    {
        public Guid PublicKey { get; }
        
        public QuestionnaireIdentity QuestionnaireId { get; }

        public Guid UserId
        {
            get;
        }

        protected CalendarEventCommand(Guid publicKey, Guid userId, QuestionnaireIdentity questionnaireId)
            : base(publicKey)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty.");
            this.UserId = userId;
            
            this.PublicKey = publicKey;
            this.OriginDate = DateTimeOffset.Now;
            this.QuestionnaireId = questionnaireId;
        }

        public DateTimeOffset OriginDate { get; }
    }
}
