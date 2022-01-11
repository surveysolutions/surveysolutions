using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent
{
    public class CompleteCalendarEventCommand : CalendarEventCommand
    {
        public CompleteCalendarEventCommand(Guid publicKey, Guid userId, QuestionnaireIdentity questionnaireId) 
            : base(publicKey, userId, questionnaireId)
        {
        }
    }
}
