using System;
using System.Threading;
using System.Threading.Tasks;
namespace WB.UI.Designer.Services
{
    public interface IOneTimeCodeStore
    {
        Task SaveAsync(OneTimeCodeEntity entity, CancellationToken ct = default);
        Task<OneTimeCodeEntity?> GetAsync(string code, CancellationToken ct = default);
        Task<bool> TryMarkAsUsedAsync(string code, DateTime usedAtUtc, CancellationToken ct = default);
    }
}
