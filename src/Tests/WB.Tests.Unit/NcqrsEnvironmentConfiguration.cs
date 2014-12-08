using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs;
using Ncqrs.Commanding;
using Ncqrs.Config;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.Infrastructure.CommandBus;

namespace WB.Tests.Unit
{
    /// <summary>
    /// Don't do this! Instead, use an IoC container and one of the extension projects to configure your environment
    /// </summary>
    public class NcqrsEnvironmentConfiguration : IEnvironmentConfiguration
    {
        private NcqrsEnvironmentConfiguration()
        {
        }

        public static void Configure()
        {
            AssemblyContext.SetupServiceLocator();

            if (NcqrsEnvironment.IsConfigured)
            {
                return;
            }

            var cfg = new NcqrsEnvironmentConfiguration();
            NcqrsEnvironment.Configure(cfg);
        }

        public bool TryGet<T>(out T result) where T : class
        {
            result = null;
            return result != null;
        }
    }
}