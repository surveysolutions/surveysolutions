﻿using System;
using System.Threading.Tasks;
using WB.Services.Export.Interview.Entities;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.User
{
    public interface IUserStorage
    {
        Task<string> GetUserNameAsync(Guid userId);
        Task<UserRoles> GetUserRoleAsync(Guid userId);
    }
}
