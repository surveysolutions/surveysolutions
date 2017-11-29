using System;
using System.Collections.Generic;
using Ncqrs;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.BoundedContexts.Headquarters.WebInterview.Impl
{
    class PauseResumeQueue : IPauseResumeQueue
    {
        private readonly TrackingSettings trackingSettings;
        private readonly IClock clock;
        private readonly Dictionary<Guid, List<TimestampedInterviewCommand>> commands = new Dictionary<Guid, List<TimestampedInterviewCommand>>();
        private readonly NamedLocker locker = new NamedLocker();

        public PauseResumeQueue(TrackingSettings trackingSettings, IClock clock)
        {
            this.trackingSettings = trackingSettings;
            this.clock = clock;
        }

        public void EnqueuePause(PauseInterviewCommand command)
        {
            DequeuePreviousAndAddNew(command, typeof(ResumeInterviewCommand));
        }

        public void EnqueueCloseBySupervisor(CloseInterviewBySupervisorCommand command)
        {
            DequeuePreviousAndAddNew(command, typeof(OpenInterviewBySupervisorCommand));
        }

        public void EnqueueResume(ResumeInterviewCommand command)
        {
            DequeuePreviousAndAddNew(command, typeof(PauseInterviewCommand));
        }

        public void EnqueueOpenBySupervisor(OpenInterviewBySupervisorCommand command)
        {
            DequeuePreviousAndAddNew(command, typeof(CloseInterviewBySupervisorCommand));
        }

        private void DequeuePreviousAndAddNew(TimestampedInterviewCommand command, Type counterCommand)
        {
            locker.RunWithLock(command.InterviewId + "pauseResume",
            () => {
                var interviewCommands = commands.GetOrAdd(command.InterviewId, () => new List<TimestampedInterviewCommand>());
                bool counteredExistingCommand = false;

                for (int i = interviewCommands.Count - 1; i >= 0; i--)
                {
                    var commandToCancel = interviewCommands[i];
                    var commandIsNewEnoughToBeCancelled = (commandToCancel.UtcTime - command.UtcTime).Duration() < trackingSettings.PauseResumeGracePeriod;
                    if (commandToCancel.GetType() == counterCommand && commandIsNewEnoughToBeCancelled)
                    {
                        interviewCommands.Remove(commandToCancel);
                        counteredExistingCommand = true;
                        break;
                    }
                }
                if (!counteredExistingCommand)
                {
                    interviewCommands.Add(command);
                }
            });
        }

        public List<InterviewCommand> DeQueueForPublish()
        {
            DateTime utcCompareTo = clock.UtcNow();
            var delayBeforePublish = trackingSettings.DelayBeforeCommandPublish;
            List<InterviewCommand> result = new List<InterviewCommand>();
            foreach (var key in commands.Keys)
            {
                locker.RunWithLock(key + "pauseResume", 
                () => {
                        var commandsList = commands[key];
                        for (int i = commandsList.Count - 1; i >= 0; i--)
                        {
                            var processedCommand = commandsList[i];
                            if ((utcCompareTo - processedCommand.UtcTime).Duration() > delayBeforePublish)
                            {
                                commandsList.Remove(processedCommand);
                                result.Add(processedCommand);
                            }
                        }
                    });
            }
            return result;
        }
    }
}