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
        object Get(Type type);

        object GetServiceWithGenericType(Type type, params Type[] genericType);

        Type GetGenericArgument();
        Type[] GetGenericArguments();
    }

    public interface IConstructorContext
    {
        T Inject<T>();
    }


}
