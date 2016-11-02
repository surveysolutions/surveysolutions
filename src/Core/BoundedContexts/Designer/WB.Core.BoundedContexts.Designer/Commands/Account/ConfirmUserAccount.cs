using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    [Serializable]
    public class ConfirmUserAccount : UserCommand
    {
        public ConfirmUserAccount(Guid userId)
            : base(userId) {}
    }
}