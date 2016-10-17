using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    [Serializable]
    public class UnlockUserAccount : UserCommand
    {
        public UnlockUserAccount(Guid userId)
            : base(userId) {}
    }
}