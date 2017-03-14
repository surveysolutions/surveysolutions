using System;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    public interface IApiTokenProvider<in TKey> where TKey : IEquatable<TKey>
    {
        Task<string> GenerateTokenAsync(TKey userId);
        Task<bool> ValidateTokenAsync(TKey userId, string token);
    }
}