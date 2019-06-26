using System.Threading.Tasks;

namespace support.Services
{
    public interface ISystemService
    {
        Task<bool> IsProcessRunning(string processName);
    }
}
