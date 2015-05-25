using System.Threading.Tasks;

namespace WB.Core.Infrastructure.CommandBus
{
    public interface ICommandService
    {
        Task ExecuteAsync(ICommand command, string origin = null);
        void Execute(ICommand command, string origin = null);
    }
}
