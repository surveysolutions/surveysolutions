using System;
using MvvmCross;
using MvvmCross.Commands;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class CoverPrefilledEntity : IDisposable
    {
        public Identity Identity { get; set; }
        public DynamicTextViewModel Title { get; set; }
        public string Answer { get; set; }
        public AttachmentViewModel Attachment { get; set; }
        public GpsLocation GpsLocation { get; set; }
        public bool IsGpsAnswered
        {
            get
            {
                if (GpsLocation == null)
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
            if (GpsLocation != null)
            {
                var externalAppLauncher = Mvx.IoCProvider.Resolve<IExternalAppLauncher>();
                externalAppLauncher.LaunchMapsWithTargetLocation(this.GpsLocation.Latitude, this.GpsLocation.Longitude);
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
