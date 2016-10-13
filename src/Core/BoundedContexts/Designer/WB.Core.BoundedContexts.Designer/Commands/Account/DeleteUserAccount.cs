using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    [Serializable]
    public class DeleteUserAccount : UserCommand
    {
        public DeleteUserAccount(Guid userId)
            : base(userId) {}
    }
}