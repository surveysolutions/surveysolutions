using System;
using Ninject.Activation;
using Ninject.Parameters;
using Ninject.Planning.Targets;

namespace WB.Infrastructure.Native.Ioc
{
    public class NonRequestScopedParameter : IParameter
    {
        public bool Equals(IParameter other)
        {
            if (other == null)
            {
                return false;
            }

            return other is NonRequestScopedParameter;
        }

        public object GetValue(IContext context, ITarget target)
        {
            throw new NotSupportedException("this parameter does not provide a value");
        }

        public string Name => typeof(NonRequestScopedParameter).Name;

        public bool ShouldInherit => true;
    }
}
