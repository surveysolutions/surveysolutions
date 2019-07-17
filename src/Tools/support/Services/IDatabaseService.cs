using System.Threading.Tasks;

namespace support
{
    public interface IDatabaseService
    {
        Task<bool> HasConnectionAsync(string connectionString);
        Task<bool> HasPermissionsAsync(string connectionString);
        Task<bool> UpdatePasswordAsync(string connectionString, string login, string passwordHash);
    }
}
