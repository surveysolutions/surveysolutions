using System;
using SQLite;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

//namespace preserved for compatibility
namespace WB.UI.Tester.Infrastructure.Internals.Security
{
    public class TesterUserIdentity : IUserIdentity, IPlainStorageEntity
    {
        [PrimaryKey]
        public string Id  { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public Guid UserId { get; set; }
    }
}