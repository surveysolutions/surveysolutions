using System;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State
{
    public class AttachmentViewModel : MvxViewModel
    {
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IAttachmentContentStorage attachmentContentStorage;
        private readonly Func<IMediaAttachment> attachmentFactory;

        private AttachmentContentMetadata attachmentContentMetadata;
        private NavigationState navigationState;
        public Identity Identity { get; private set; }

        public string Tag => Identity.ToString();

        private const string ImageMimeType = "image/";
        private const string VideoMimeType = "video/";
        private const string AudioMimeType = "audio/";
        private const string PdfMimeType = "application/pdf";

        public AttachmentViewModel(
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            IAttachmentContentStorage attachmentContentStorage,
            Func<IMediaAttachment> attachmentFactory)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.attachmentContentStorage = attachmentContentStorage;
            this.attachmentFactory = attachmentFactory;
        }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            this.navigationState = navigationState ?? throw new ArgumentNullException(nameof(navigationState));
            this.Identity = entityIdentity ?? throw new ArgumentNullException(nameof(entityIdentity));

            var interview = this.interviewRepository.Get(interviewId);
            IQuestionnaire questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            var attachment = questionnaire.GetAttachmentForEntity(entityIdentity.Id);

            if (attachment != null)
            {
                this.attachmentContentMetadata = this.attachmentContentStorage.GetMetadata(attachment.ContentId);

                if (IsImage)
                {
                    this.Image = this.attachmentContentStorage.GetContent(attachment.ContentId);
                    this.RaisePropertyChanged(nameof(Image));
                    return;
                }

                var backingFile = this.attachmentContentStorage.GetFileCacheLocation(attachment.ContentId);
                if (string.IsNullOrWhiteSpace(backingFile)) return;


                if (IsVideo || IsAudio)
                {
                    var media = this.attachmentFactory();
                    media.ContentPath = backingFile;

                    if (IsVideo)
                    {
                        this.Video = media;
                        this.RaisePropertyChanged(nameof(Video));
                    }

                    if (IsAudio)
                    {
                        this.Audio = media;
                        this.RaisePropertyChanged(nameof(Audio));
                    }

                    return;
                }

                if (IsPdf)
                {
                    this.ContentPath = backingFile;
                    this.RaisePropertyChanged(nameof(ContentPath));
                }
            }
        }

        public IMediaAttachment Audio { get; private set; }
        public IMediaAttachment Video { get; private set; }
        public byte[] Image { get; private set; }

        public string ContentPath { get; set; }

        public bool IsImage => this.attachmentContentMetadata != null
            && this.attachmentContentMetadata.ContentType.StartsWith(ImageMimeType, StringComparison.OrdinalIgnoreCase);

        public bool IsVideo => this.attachmentContentMetadata != null
            && this.attachmentContentMetadata.ContentType.StartsWith(VideoMimeType, StringComparison.OrdinalIgnoreCase);

        public bool IsAudio => this.attachmentContentMetadata != null
            && this.attachmentContentMetadata.ContentType.StartsWith(AudioMimeType, StringComparison.OrdinalIgnoreCase);

        public bool IsPdf => this.attachmentContentMetadata != null
            && this.attachmentContentMetadata.ContentType.StartsWith(PdfMimeType, StringComparison.OrdinalIgnoreCase);

        public IMvxAsyncCommand ShowPdf => new MvxAsyncCommand(NavigateToPdfAsync);

        private async Task NavigateToPdfAsync()
        {
            await this.navigationState.NavigateTo(NavigationIdentity.CreateForPdfViewByStaticText(this.Identity));
        }

        public override void ViewDestroy(bool viewFinishing = true)
        {
            this.Video?.Release();
            this.Audio?.Release();
            this.ContentPath = null;
            base.ViewDestroy(viewFinishing);
        }
    }
}
