using System;

namespace Ncqrs.Config
{
    public interface IEnvironmentConfiguration
    {
        Boolean TryGet<T>(out T result) where T : class;
    }
}