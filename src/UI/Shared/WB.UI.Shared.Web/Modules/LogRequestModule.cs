using System;
using Autofac.Core;

namespace WB.UI.Shared.Web.Modules
{

    public class LogRequestModule : Autofac.Module
    {
        public int depth = 0;

        private string trace;

        protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry,
            IComponentRegistration registration)
        {
            registration.Preparing += RegistrationOnPreparing;
            registration.Activating += RegistrationOnActivating;
            base.AttachToComponentRegistration(componentRegistry, registration);
        }

        private string GetPrefix()
        {
            return new string('-', Math.Max(depth * 2, 2));
        }

        private void RegistrationOnPreparing(object sender, PreparingEventArgs preparingEventArgs)
        {
            trace += ($"{GetPrefix()}Resolving  {preparingEventArgs.Component.Activator.LimitType} \r\n");
            depth++;
        }

        private void RegistrationOnActivating(object sender, ActivatingEventArgs<object> activatingEventArgs)
        {
            depth--;
            trace += ($"{GetPrefix()}Activating {activatingEventArgs.Component.Activator.LimitType} lifetime:{activatingEventArgs.Component.Lifetime}\r\n");
        }
    }
}
