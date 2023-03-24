using System;
using System.Threading.Tasks;

namespace WB.Services.Export.User
{
    public interface IUserStorage
    {
        Task<User?> GetUserAsync(Guid userId);
    }
}
