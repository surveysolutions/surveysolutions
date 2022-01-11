using System;
using Ncqrs.Commanding;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent
{
    public class RestoreCalendarEventCommand : CalendarEventCommand
    {
        public RestoreCalendarEventCommand(Guid publicKey, Guid userId, QuestionnaireIdentity questionnaireIdentity)
            :base(publicKey, userId, questionnaireIdentity)
        {
        }
    }
}
