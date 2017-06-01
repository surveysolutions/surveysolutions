using System;
using WB.Core.GenericSubdomains.Utils;

namespace WB.Core.Infrastructure.Transactions
{
    internal static class ActionExtensions
    {
        public static Func<Unit> ToFunc(this Action action)
        {
            return () =>
            {
                action.Invoke();
                return Unit.Value;
            };
        }
    }
}