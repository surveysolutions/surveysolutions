using System;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UserTests
{
    internal class UserTestsContext
    {
        public static User CreateUser() => new User();
    }
}
