using System;

namespace WB.Core.Infrastructure.Modularity
{
    public interface IModuleContext
    {
        T Resolve<T>();

        Type MemberDeclaringType { get; }

        T Inject<T>();

        T Get<T>();
        T Get<T>(string name);

        object GetServiceWithGenericType(Type type, Type genericType);

        Type GetGenericArgument();
    }

    public interface IConstructorContext
    {
        T Inject<T>();
    }


}