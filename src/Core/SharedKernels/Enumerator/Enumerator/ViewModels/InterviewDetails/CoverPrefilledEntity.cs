using System;
using Main.Core.Entities.SubEntities;
using MvvmCross;
using MvvmCross.Commands;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class CoverPrefilledEntity : IDisposable
    {
        public Identity Identity { get; set; }
        public DynamicTextViewModel Title { get; set; }
        public string Answer { get; set; }
        public AttachmentViewModel Attachment { get; set; }
        public GeoLocation GeoLocation { get; set; }
        public bool IsGpsAnswered
        {
            get
            {
                if (GeoLocation == null)
                    return false;

                var settings = Mvx.IoCProvider.Resolve<IEnumeratorSettings>();
                if (!settings.ShowLocationOnMap)
                    return false;

                var googleApiService = Mvx.IoCProvider.Resolve<IGoogleApiService>();
                return googleApiService.GetPlayServicesConnectionStatus() == GoogleApiConnectionStatus.Success;
            }
        }

        public IMvxCommand AnswerClickedCommand => new MvxCommand(AnswerClickedHandler);

        private void AnswerClickedHandler()
        {
            if (GeoLocation != null)
            {
                var externalAppLauncher = Mvx.IoCProvider.Resolve<IExternalAppLauncher>();
                externalAppLauncher.LaunchMapsWithTargetLocation(this.GeoLocation.Latitude, this.GeoLocation.Longitude);
            }
        }

        public void Dispose()
        {
            Title?.Dispose();
            Attachment?.ViewDestroy();
            Attachment?.Dispose();
        }
    }
}
