using System;
using Ncqrs.Commanding;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent
{
    public class UpdateCalendarEventCommand : CalendarEventCommand
    {
        public UpdateCalendarEventCommand(Guid publicKey, 
            Guid userId,
            DateTimeOffset start,
            string startTimezone,
            string comment,
            QuestionnaireIdentity questionnaireIdentity):base(publicKey, userId, questionnaireIdentity)
        {
            this.Start = start;
            this.StartTimezone = startTimezone;
            this.Comment = comment;
        }

        public string Comment { get; set; }
        public DateTimeOffset Start { get; set; }
        public string StartTimezone { get; set; }
    }
}
