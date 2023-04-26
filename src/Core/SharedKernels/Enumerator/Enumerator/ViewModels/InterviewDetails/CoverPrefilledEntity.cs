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
        public GeoPosition GeoPosition { get; set; }

        public IMvxCommand AnswerClickedCommand => new MvxCommand(AnswerClickedHandler);

        private void AnswerClickedHandler()
        {
            if (GeoPosition != null)
            {
                var externalAppLauncher = Mvx.IoCProvider.Resolve<IExternalAppLauncher>();
                externalAppLauncher.LaunchMapsWithTargetLocation(this.GeoPosition.Latitude, this.GeoPosition.Longitude);
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
