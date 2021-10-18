using System;
using System.Linq;
using System.Reflection;
using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Core.Resolving.Pipeline;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.UI.Shared.Enumerator.Services.Logging;
using Module = Autofac.Module;

namespace WB.UI.Shared.Enumerator
{
    public class EnumeratorLoggingModule : Module
    {
        private readonly LoggingMiddleware middleware = new LoggingMiddleware();
        
        protected override void AttachToComponentRegistration(IComponentRegistryBuilder componentRegistry, IComponentRegistration registration)
        {
            // Handle constructor parameters.
            
            // Attach to the registration's pipeline build.
            registration.PipelineBuilding += (sender, pipeline) =>
            {
                // Add our middleware to the pipeline.
                pipeline.Use(middleware);
            };
        }
    }
    
    public class LoggingMiddleware : IResolveMiddleware
    {
        public PipelinePhase Phase => PipelinePhase.ParameterSelection;

        public void Execute(ResolveRequestContext context, Action<ResolveRequestContext> next)
        {
            // Add our parameters.
            context.ChangeParameters(context.Parameters.Union(
                new[]
                {
                    new ResolvedParameter(
                        (p, i) => p.ParameterType == typeof(ILogger),
                        (p, i) => new NLogLogger(p.Member.DeclaringType)
                    ),
                }));

            // Continue the resolve.
            next(context);

            // Has an instance been activated?
            if (context.NewInstanceActivated)
            {
                var instanceType = context.Instance.GetType();

                // Get all the injectable properties to set.
                // If you wanted to ensure the properties were only UNSET properties,
                // here's where you'd do it.
                var properties = instanceType
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.PropertyType == typeof(ILogger) && p.CanWrite && p.GetIndexParameters().Length == 0);

                // Set the properties located.
                foreach (var propToSet in properties)
                {
                    propToSet.SetValue(context.Instance, new NLogLogger(instanceType), null);
                }
            }
        }
    }
}
