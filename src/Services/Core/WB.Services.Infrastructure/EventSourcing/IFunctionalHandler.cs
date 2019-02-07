using System.Threading;
using System.Threading.Tasks;

namespace WB.Services.Infrastructure.EventSourcing
{
    /// <summary>
    /// Stateful handler for events.
    /// Functional handler is the only one that responsible for own state
    /// </summary>
    public interface IFunctionalHandler
    {
        Task SaveStateAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Functional handlers marked with this interface will be executed earlier than other handlers
    /// </summary>
    public interface IHighPriorityFunctionalHandler: IFunctionalHandler {  }
}
