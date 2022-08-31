using System;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class PhotoViewViewModel : BaseViewModel<PhotoViewViewModelArgs>
    {
        private readonly ILogger logger;
        private readonly IImageFileStorage imageFileStorage;
        private readonly IAttachmentContentStorage attachmentContentStorage;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private byte[] answer;

        private Guid interviewId;
        private string fileName;
        private Guid? attachmentId = null;

        private const string StateKey = "interviewPhotoPreview";

        public PhotoViewViewModel(IPrincipal principal, 
            IViewModelNavigationService viewModelNavigationService,
            ILogger logger, IImageFileStorage imageFileStorage, IAttachmentContentStorage attachmentContentStorage,
            IStatefulInterviewRepository interviewRepository, IQuestionnaireStorage questionnaireStorage) 
            : base(principal, viewModelNavigationService)
        {
            this.logger = logger;
            this.imageFileStorage = imageFileStorage;
            this.attachmentContentStorage = attachmentContentStorage;
            this.interviewRepository = interviewRepository;
            this.questionnaireStorage = questionnaireStorage;
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
            this.attachmentId = parameter.AttachmentId;
        }

        public override async Task Initialize()
        {
            await base.Initialize();
            if (interviewId == Guid.Empty) throw new ArgumentException(nameof(interviewId));

            if (!string.IsNullOrEmpty(this.fileName))
            {
                this.Answer = await this.imageFileStorage.GetInterviewBinaryDataAsync(this.interviewId, this.fileName);
            }
            else if (this.attachmentId.HasValue)
            {
                var interview = this.interviewRepository.GetOrThrow(interviewId.FormatGuid());
                var questionnaire = questionnaireStorage.GetQuestionnaireOrThrow(interview.QuestionnaireIdentity, interview.Language);
                var attachment = questionnaire.GetAttachmentById(this.attachmentId.Value);
                this.Answer = this.attachmentContentStorage.GetContent(attachment.ContentId);
            }

            if (this.Answer == null)
                throw new ArgumentException(nameof(Answer));
        }
    }
}
