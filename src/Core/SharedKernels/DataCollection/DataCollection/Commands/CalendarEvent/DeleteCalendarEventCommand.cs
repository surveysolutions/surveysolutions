using System;
using Ncqrs.Commanding;

namespace WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent
{
    public class DeleteCalendarEventCommand : CalendarEventCommand
    {
        public DeleteCalendarEventCommand(Guid publicKey, 
            Guid userId):base(publicKey, userId)
        {
        }
    }
}
