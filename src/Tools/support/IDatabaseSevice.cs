using System.Threading.Tasks;

namespace support
{
    public interface IDatabaseSevice
    {
        Task<bool> HasConnectionAsync(string connectionString);
        Task<bool> HasPermissionsAsync(string connectionString);
    }
}