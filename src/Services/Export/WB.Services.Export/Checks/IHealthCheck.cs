using System.Threading.Tasks;

namespace WB.Services.Export.Checks
{
    public interface IHealthCheck
    {
        Task<bool> CheckAsync();
        string Name { get; }
    }
}
