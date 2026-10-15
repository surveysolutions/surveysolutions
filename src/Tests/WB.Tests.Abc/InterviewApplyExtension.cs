using System;
using System.Collections.Concurrent;
using System.Reflection;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Abc
{
    public static class InterviewApplyExtension
    {
        static readonly ConcurrentDictionary<Type, MethodInfo> reflectionCache = new ConcurrentDictionary<Type, MethodInfo>();

        /// <summary>
        /// Do not use this method in new tests! use Commands or InitializeFromHistory
        /// </summary>
        /// <param name="interview"></param>
        /// <param name="event"></param>
        public static void Apply(this Interview interview, IEvent @event)
        {
            var method =
                reflectionCache.GetOrAdd(@event.GetType(), type =>
                    typeof(Interview).GetMethod(
                        "Apply",
                        BindingFlags.Instance | BindingFlags.NonPublic,
                        Type.DefaultBinder,
                        new[] {type},
                        null));

            method.Invoke(interview, new[] { @event });

            interview.DiscardChanges();
        }
    }
}
