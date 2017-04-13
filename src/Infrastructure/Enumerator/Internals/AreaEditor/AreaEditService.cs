using System.Threading.Tasks;
using MvvmCross.Platform.Droid.Platform;
using Plugin.Permissions.Abstractions;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Infrastructure.Shared.Enumerator.Internals
{
    internal class AreaEditService : IAreaEditService
    {
        private readonly IMvxAndroidCurrentTopActivity androidCurrentTopActivity;
        private readonly IPermissions permissions;

        public AreaEditService(IMvxAndroidCurrentTopActivity androidCurrentTopActivity, IPermissions permissions)
        {
            this.androidCurrentTopActivity = androidCurrentTopActivity;
            this.permissions = permissions;
        }

        public async Task<AreaEditResult> EditAreaAsync(string area)
        {
            await this.permissions.AssureHasPermission(Permission.Location);

            return new AreaEditResult();
        }
    }
}