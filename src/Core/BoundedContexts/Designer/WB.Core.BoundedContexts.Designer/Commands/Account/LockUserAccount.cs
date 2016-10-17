using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    [Serializable]
    public class LockUserAccount : UserCommand
    {
        public LockUserAccount(Guid userId)
            : base(userId) {}
    }
}