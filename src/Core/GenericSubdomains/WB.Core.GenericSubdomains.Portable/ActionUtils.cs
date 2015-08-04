using System;
using System.Collections.Generic;

namespace WB.Core.GenericSubdomains.Utils
{
    public static class ActionUtils
    {
        public static void ExecuteInIndependentTryCatchBlocks(params Action[] actions)
        {
            var exceptions = new List<Exception>();

            foreach (var action in actions)
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception exception)
                {
                    exceptions.Add(exception);
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
        }
    }
}