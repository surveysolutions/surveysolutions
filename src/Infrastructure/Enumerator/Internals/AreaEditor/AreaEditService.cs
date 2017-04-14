using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using MvvmCross.Platform.Droid.Platform;
using MWBarcodeScanner;
using Plugin.Permissions.Abstractions;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Infrastructure.Shared.Enumerator.Internals.AreaEditor;
using AreaEditResult = WB.Core.SharedKernels.Enumerator.Services.Infrastructure.AreaEditResult;

namespace WB.Infrastructure.Shared.Enumerator.Internals
{
    internal class AreaEditService : IAreaEditService
    {
        private readonly IMvxAndroidCurrentTopActivity androidCurrentTopActivity;
        private readonly IPermissions permissions;

        private ManualResetEvent areaEditResultResetEvent = new ManualResetEvent(false);

        public AreaEditService(IMvxAndroidCurrentTopActivity androidCurrentTopActivity, IPermissions permissions)
        {
            this.androidCurrentTopActivity = androidCurrentTopActivity;
            this.permissions = permissions;
        }

        public async Task<AreaEditResult> EditAreaAsync(string area)
        {
            await this.permissions.AssureHasPermission(Permission.Location);

            return await EditArea(area);
        }

        private Task<AreaEditResult> EditArea(string area)
        {
            return Task.Factory.StartNew<AreaEditResult>((Func<AreaEditResult>)(() =>
            {
                try
                {
                    AreaEditorResult result = null;

                    this.areaEditResultResetEvent.Reset();
                    ManualResetEvent waitEditAreaResetEvent = new ManualResetEvent(false);

                    Intent intent = new Intent(androidCurrentTopActivity.Activity, typeof(AreaEditorActivity));

                    intent.PutExtra(Intent.ExtraText, area);

                    AreaEditorActivity.OnAreaEditCompleted += (editResult =>
                    {
                        result = editResult;
                        waitEditAreaResetEvent.Set();
                    });
                    
                    this.androidCurrentTopActivity.Activity.StartActivity(intent);

                    waitEditAreaResetEvent.WaitOne();

                    return result != null ? new AreaEditResult() { Area = result.Area} : null;
                }
                catch (System.Exception ex)
                {
                    return (AreaEditResult)null;
                }
            }));
        }
    }
}