using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.GenericSubdomains.Utils
{
    public static class ActionUtils
    {
        public static void ExecuteInIndependentTryCatchBlocks<TExecutee>(IEnumerable<TExecutee> executees, Action<TExecutee> execute)
        {
            ExecuteInIndependentTryCatchBlocks(
                executees.Select(
                    executee => (Action) (() => execute(executee))));
        }

        public static void ExecuteInIndependentTryCatchBlocks(IEnumerable<Action> actions)
        {
            var exceptions = new List<Exception>();

            foreach (Action action in actions)
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