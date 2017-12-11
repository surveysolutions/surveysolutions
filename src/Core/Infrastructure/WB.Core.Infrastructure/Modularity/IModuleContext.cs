using System;

namespace WB.Core.Infrastructure.Modularity
{
    public interface IModuleContext
    {
        T Resolve<T>();

        Type MemberDeclaringType { get; }

        T Inject<T>();
    }

    public interface IConstructorContext
    {
        T Inject<T>();
    }


}