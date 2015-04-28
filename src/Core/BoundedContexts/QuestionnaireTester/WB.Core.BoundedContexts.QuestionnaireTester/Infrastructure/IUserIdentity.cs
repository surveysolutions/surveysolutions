using System;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure
{
    public interface IUserIdentity
    {
        string Name { get; }
        string Password { get; }
        Guid UserId { get; }
    }
}