﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.BoundedContexts.Headquarters.Users
{
    public interface IUserRepository
    {
        IQueryable<HqUser> Users { get; }
        HqRole FindRole(Guid id);
        Task<HqUser> FindByIdAsync(Guid userId, CancellationToken cancellationToken = new CancellationToken());
        Task<HqUser> FindByNameAsync(string userName, CancellationToken cancellationToken = new CancellationToken());
        HqUser FindById(Guid userId);
        Task<string> GetEmailAsync(HqUser user);
        Task<HqUser> FindByEmailAsync(string email, CancellationToken cancellationToken = new CancellationToken());
    }
}
