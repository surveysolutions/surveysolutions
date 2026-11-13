using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.BoundedContexts.Tester.Implementation.Services
{
    class ExecutedCommandsStorage : IExecutedCommandsStorage
    {
        private readonly ConcurrentDictionary<Guid, List<InterviewCommand>> commands = new ConcurrentDictionary<Guid, List<InterviewCommand>>();

        public void Add(Guid interviewId, InterviewCommand command)
        {
            var storedCommands = commands.GetOrAdd(interviewId, id => new List<InterviewCommand>());
            lock (storedCommands)
                storedCommands.Add(command);
        }

        public List<InterviewCommand> Get(Guid interviewId)
        {
            if (!this.commands.TryGetValue(interviewId, out var storedCommands))
                return new List<InterviewCommand>();

            lock (storedCommands)
                return new List<InterviewCommand>(storedCommands);
        }

        public void Clear(Guid interviewId)
        {
            this.commands.TryRemove(interviewId, out _);
        }
    }
}
