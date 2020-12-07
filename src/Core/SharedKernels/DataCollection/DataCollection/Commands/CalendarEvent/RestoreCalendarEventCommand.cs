using System;
using Ncqrs.Commanding;

namespace WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent
{
    public class RestoreCalendarEventCommand : CalendarEventCommand
    {
        public RestoreCalendarEventCommand(Guid publicKey, 
            Guid userId):base(publicKey, userId)
        {
        }
    }
}
