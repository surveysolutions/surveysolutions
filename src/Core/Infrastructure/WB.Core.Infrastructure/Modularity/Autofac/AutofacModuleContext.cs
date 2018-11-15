using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Core;

namespace WB.Core.Infrastructure.Modularity.Autofac
{
    public class AutofacModuleContext : IModuleContext
    {
        private readonly IComponentContext ctx;
        private readonly IEnumerable<Parameter> parameters;

        public AutofacModuleContext(IComponentContext ctx, IEnumerable<Parameter> parameters)
        {
            this.ctx = ctx;
            this.parameters = parameters;
        }

        public T Resolve<T>()
        {
            return ctx.Resolve<T>();
        }

        public Type MemberDeclaringType
        {
            get
            {
                var targetType = parameters.OfType<NamedParameter>()
                    .FirstOrDefault(np => np.Name == ResolutionExtensions.PropertyInjectedInstanceTypeNamedParameter && np.Value is Type);

                return (Type) targetType?.Value;
            }
        }

        public T Inject<T>()
        {
            return Resolve<T>();
        }

        public T Get<T>()
        {
            return Resolve<T>();
        }

        public T Get<T>(string name)
        {
            return Resolve<T>();
        }

        public object Get(Type type)
        {
            return this.ctx.Resolve(type);
        }

        public object GetServiceWithGenericType(Type type, params Type[] genericType)
        {
            return ctx.Resolve(type.MakeGenericType(genericType));
        }

        public Type GetGenericArgument()
        {
            var targetType = parameters.OfType<NamedParameter>()
                .FirstOrDefault(np => np.Value is Type);

            return (Type) targetType?.Value;
        }

        public Type[] GetGenericArguments()
        {
            return parameters.OfType<NamedParameter>().Select(np => np.Value).OfType<Type>().ToArray();
        }
    }
}
