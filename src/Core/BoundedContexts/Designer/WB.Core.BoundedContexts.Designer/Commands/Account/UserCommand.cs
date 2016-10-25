using System;
using Ncqrs.Commanding;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    public abstract class UserCommand : CommandBase
    {
        protected UserCommand(Guid userId)
            : base(userId)
        {
            this.UserId = userId;
        }

        public Guid UserId { get; private set; }
    }
}
