using System;
using SQLite;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

//namespace preserved for compatibility
// ReSharper disable once CheckNamespace
namespace WB.UI.Tester.Infrastructure.Internals.Security
{
    public class TesterUserIdentity : IUserIdentity, IPlainStorageEntity
    {
        [PrimaryKey]
        public string Id  { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string PasswordHash { get; set; }
        public string Token { get; set; }
        public Guid UserId { get; set; }
        public string Email { get; set; }
    }
}
