using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace WB.Services.Export.Handlers
{
    /// <summary>
    /// Stateful handler for events.
    /// Functional handler is the only one that responsible for own state
    /// </summary>
    public interface IFunctionalHandler
    {
        Task SaveStateAsync(CancellationToken cancellationToken = default);
    }
}
