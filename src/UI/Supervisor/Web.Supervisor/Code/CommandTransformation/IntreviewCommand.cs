using System;
using Ncqrs.Commanding;

namespace Web.Supervisor.Code.CommandTransformation
{
    internal class IntreviewCommand : ICommand
    {
        public Guid InterviewId { get; set; }
        public Guid UserId { get; set; }
        public Guid CommandIdentifier { get; set; }
        public long? KnownVersion { get; set; }
    }
}