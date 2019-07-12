using System.Threading.Tasks;

namespace WB.Core.Infrastructure.Services
{
    public interface IBackgroundJob<T>
    {
        Task ExecuteAsync(T item);
    }
}
