using System;

namespace WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent
{
    public class CompleteCalendarEventCommand : CalendarEventCommand
    {
        public CompleteCalendarEventCommand(Guid publicKey, Guid userId) : base(publicKey, userId)
        {
        }
    }
}
