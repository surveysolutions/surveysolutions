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
            var storedCommands = commands.GetOrAdd(interviewId, new List<InterviewCommand>());
            storedCommands.Add(command);
        }

        public List<InterviewCommand> Get(Guid interviewId)
        {
            return this.commands.ContainsKey(interviewId) ? this.commands[interviewId] : new List<InterviewCommand>();
        }

        public void Clear(Guid interviewId)
        {
            this.commands.TryRemove(interviewId, out _);
        }
    }
}
