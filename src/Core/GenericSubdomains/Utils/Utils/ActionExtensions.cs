using System;

namespace WB.Core.GenericSubdomains.Utils
{
    public static class ActionExtensions
    {
        public static Func<Unit> ToFunc(this Action action)
        {
            return () =>
            {
                action.Invoke();
                return Unit.Empty;
            };
        }
    }
}