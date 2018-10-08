using System.Threading.Tasks;

namespace WB.Services.Infrastructure.Health
{
    public interface IHealthCheck
    {
        Task<bool> CheckAsync();
        string Name { get; }
    }
}
