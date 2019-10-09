using System;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    public interface IApiTokenProvider
    {
        Task<string> GenerateTokenAsync(Guid userId);
        Task<bool> ValidateTokenAsync(Guid userId, string token);
    }
}
