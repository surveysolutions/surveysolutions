using System;
using SQLite.Net.Attributes;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.UI.Tester.Infrastructure.Internals.Security
{
    internal class TesterUserIdentity : IUserIdentity, IPlainStorageEntity
    {
        [PrimaryKey]
        public string Id  { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public Guid UserId { get; set; }
    }
}