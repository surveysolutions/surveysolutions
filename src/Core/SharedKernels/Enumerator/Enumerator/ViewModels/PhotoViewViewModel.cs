using System;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class PhotoViewViewModel : BaseViewModel<PhotoViewViewModelArgs>
    {
        private readonly ILogger logger;
        private readonly IInterviewFileStorage imageFileStorage;
        private byte[] answer;

        private Guid interviewId;
        private string fileName;

        private const string StateKey = "interviewPhotoPreview";

        public PhotoViewViewModel(IPrincipal principal, IViewModelNavigationService viewModelNavigationService,
            ILogger logger, IInterviewFileStorage imageFileStorage) 
            : base(principal, viewModelNavigationService)
        {
            this.logger = logger;
            this.imageFileStorage = imageFileStorage;
        }

        public byte[] Answer
        {
            get => this.answer;
            set
            {
                this.answer = value;
                this.RaisePropertyChanged();
            }
        }

        public override void Prepare(PhotoViewViewModelArgs parameter)
        {
            this.interviewId = parameter.InterviewId;
            this.fileName = parameter.FileName;
        }

        public override async Task Initialize()
        {
            await base.Initialize();
            if (interviewId == Guid.Empty) throw new ArgumentException(nameof(interviewId));

            this.Answer = this.imageFileStorage.GetInterviewBinaryData(this.interviewId, this.fileName);

            if (this.Answer == null)
                throw new ArgumentException(nameof(Answer));
        }
    }
}
