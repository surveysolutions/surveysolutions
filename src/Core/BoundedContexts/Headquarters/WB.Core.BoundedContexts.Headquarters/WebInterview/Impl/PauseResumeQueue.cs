using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Ncqrs;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;

namespace WB.Core.BoundedContexts.Headquarters.WebInterview.Impl
{
    class PauseResumeQueue : IPauseResumeQueue
    {
        private readonly TrackingSettings trackingSettings;
        private readonly IClock clock;
        private readonly ConcurrentDictionary<Guid, TimestampedInterviewCommand> commands = new ConcurrentDictionary<Guid, TimestampedInterviewCommand>();

        private readonly Dictionary<Type, Type> counterCommands = new Dictionary<Type, Type>
        {
            {typeof(ResumeInterviewCommand), typeof(PauseInterviewCommand)},
            {typeof(PauseInterviewCommand), typeof(ResumeInterviewCommand)},
            {typeof(OpenInterviewBySupervisorCommand), typeof(CloseInterviewBySupervisorCommand)},
            {typeof(CloseInterviewBySupervisorCommand), typeof(OpenInterviewBySupervisorCommand)},
        };


        public PauseResumeQueue(TrackingSettings trackingSettings, IClock clock)
        {
            this.trackingSettings = trackingSettings;
            this.clock = clock;
        }

        public void EnqueuePause(PauseInterviewCommand command)
        {
            DequeuePreviousAndAddNew(command);
        }

        public void EnqueueCloseBySupervisor(CloseInterviewBySupervisorCommand command)
        {
            DequeuePreviousAndAddNew(command);
        }

        public void EnqueueResume(ResumeInterviewCommand command)
        {
            DequeuePreviousAndAddNew(command);
        }

        public void EnqueueOpenBySupervisor(OpenInterviewBySupervisorCommand command)
        {
            DequeuePreviousAndAddNew(command);
        }

        private void DequeuePreviousAndAddNew(TimestampedInterviewCommand command)
        {
            commands[Guid.NewGuid()] = command;
        }

        public List<TimestampedInterviewCommand> DeQueueForPublish()
        {
            List<TimestampedInterviewCommand> result = new List<TimestampedInterviewCommand>();

            var commandToBePublished = commands.Where(x => (clock.UtcNow() - x.Value.OriginDate.UtcDateTime).Duration() > trackingSettings.DelayBeforeCommandPublish).ToList();

            foreach (var commandToBeAdded in commandToBePublished)
            {
                var counterCommand = FindCounterCommand(result, commandToBeAdded);
                if (counterCommand != null)
                {
                    result.Remove(counterCommand);
                }
                else
                {
                    result.Add(commandToBeAdded.Value);
                }

                commands.TryRemove(commandToBeAdded.Key, out _);
            }

            return result;
        }

        private TimestampedInterviewCommand FindCounterCommand(List<TimestampedInterviewCommand> result, KeyValuePair<Guid, TimestampedInterviewCommand> commandToBeAdded)
        {
            return result.FirstOrDefault(x =>
                x.InterviewId == commandToBeAdded.Value.InterviewId &&
                (x.OriginDate - commandToBeAdded.Value.OriginDate).Duration() < this.trackingSettings.PauseResumeGracePeriod &&
                counterCommands[commandToBeAdded.Value.GetType()] == x.GetType());
        }
    }
}
