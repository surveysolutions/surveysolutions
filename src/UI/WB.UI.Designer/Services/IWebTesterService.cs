using System;
using System.Threading;
using System.Threading.Tasks;

namespace WB.UI.Designer.Services
{
    public interface IWebTesterService
    {
        /// <summary>
        /// Creates a cryptographically-strong one-time authorization code scoped to
        /// <paramref name="questionnaireId"/> and stores it for later exchange by WebTester.
        /// Returns the code (NOT a JWT).
        /// </summary>
        Task<string> CreateOneTimeCodeAsync(
            Guid questionnaireId,
            string? userId,
            string correlationId,
            CancellationToken ct = default);
    }
}
