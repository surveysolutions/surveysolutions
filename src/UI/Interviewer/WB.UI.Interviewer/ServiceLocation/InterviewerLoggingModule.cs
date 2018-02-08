﻿using System.Linq;
using Autofac;
using Autofac.Core;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.UI.Shared.Enumerator.Services.Logging;

namespace WB.UI.Interviewer.ServiceLocation
{
    public class InterviewerLoggingModule : Module
    {
        private static void OnComponentPreparing(object sender, PreparingEventArgs e)
        {
            e.Parameters = e.Parameters.Union(
                new[]
                {
                    new ResolvedParameter(
                        (p, i) => p.ParameterType == typeof(ILogger),
                        (p, i) => new NLogLogger(p.Member.DeclaringType)
                    ),
                });
        }

        protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
        {
            // Handle constructor parameters.
            registration.Preparing += OnComponentPreparing;
        }
    }
}