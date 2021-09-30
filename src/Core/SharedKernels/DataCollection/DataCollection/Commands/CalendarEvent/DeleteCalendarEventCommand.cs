using System;
using Ncqrs.Commanding;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent
{
    public class DeleteCalendarEventCommand : CalendarEventCommand
    {
        public DeleteCalendarEventCommand(Guid publicKey, Guid userId, QuestionnaireIdentity questionnaireIdentity)
            : base(publicKey, userId, questionnaireIdentity)
        {
        }
    }
}
