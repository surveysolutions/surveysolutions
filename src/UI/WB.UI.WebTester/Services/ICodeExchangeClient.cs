using System.Threading;
using System.Threading.Tasks;

namespace WB.UI.WebTester.Services
{
    public interface ICodeExchangeClient
    {
        /// <summary>
        /// Exchanges a one-time code for a short-lived delegated JWT by calling
        /// Service A (Designer) over a backend channel.
        /// Returns null if the exchange fails for any reason.
        /// </summary>
        Task<ExchangeCodeResponse?> ExchangeAsync(string code, CancellationToken ct = default);
    }
}

