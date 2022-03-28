using System;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State
{
    public class AttachmentViewModel : MvxViewModel,
        IAsyncViewModelEventHandler<VariablesChanged>,
        IDisposable
    {
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IViewModelEventRegistry eventRegistry;
        private readonly IAttachmentContentStorage attachmentContentStorage;
        private readonly Func<IMediaAttachment> attachmentFactory;
        private readonly IInterviewPdfService pdfService;

        private AttachmentContentMetadata attachmentContentMetadata;
        private NavigationState navigationState;
        private Guid? attachmentId;
        public Identity Identity { get; private set; }

        public string Tag => "attachment_" + Identity.ToString();

        private const string ImageMimeType = "image/";
        private const string VideoMimeType = "video/";
        private const string AudioMimeType = "audio/";
        private const string PdfMimeType = "application/pdf";

        public AttachmentViewModel(
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            IViewModelEventRegistry eventRegistry,
            IAttachmentContentStorage attachmentContentStorage,
            Func<IMediaAttachment> attachmentFactory,
            IInterviewPdfService pdfService)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.eventRegistry = eventRegistry;
            this.attachmentContentStorage = attachmentContentStorage;
            this.attachmentFactory = attachmentFactory;
            this.pdfService = pdfService;
        }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            this.navigationState = navigationState ?? throw new ArgumentNullException(nameof(navigationState));
            this.Identity = entityIdentity ?? throw new ArgumentNullException(nameof(entityIdentity));

            this.eventRegistry.Subscribe(this, interviewId);
            BindAttachment().WaitAndUnwrapException();
        }

        private async Task BindAttachment()
        {
            var interview = this.interviewRepository.GetOrThrow(navigationState.InterviewId);
            var newAttachment = interview.GetAttachmentForEntity(Identity);
            if (newAttachment == null)
            {
                await BindNoAttachment();
                return;
            }
            
            if (this.attachmentId != newAttachment)
            {
                this.attachmentId = newAttachment;
                IQuestionnaire questionnaire =
                    this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
                var attachment = questionnaire.GetAttachmentById(this.attachmentId.Value);
                
                this.attachmentContentMetadata = this.attachmentContentStorage.GetMetadata(attachment.ContentId);

                if (IsImage)
                {
                    this.Image = this.attachmentContentStorage.GetContent(attachment.ContentId);
                }

                var backingFile = this.attachmentContentStorage.GetFileCacheLocation(attachment.ContentId);
                if (!string.IsNullOrWhiteSpace(backingFile))
                {
                    if (IsVideo || IsAudio)
                    {
                        var media = this.attachmentFactory();
                        media.ContentPath = backingFile;

                        if (IsVideo)
                        {
                            this.Video = media;
                        }

                        if (IsAudio)
                        {
                            this.Audio = media;
                        }

                    }

                    if (IsPdf)
                    {
                        this.ContentPath = backingFile;
                    }
                }
                
                await RaiseAllPropertiesChanged();
            }
        }

        private async Task BindNoAttachment()
        {
            this.attachmentId = null;
            this.attachmentContentMetadata = null;
            await RaiseAllPropertiesChanged();
        }

        public IMediaAttachment Audio { get; private set; }
        public IMediaAttachment Video { get; private set; }
        public byte[] Image { get; private set; }

        public string ContentPath { get; set; }

        public bool IsImage => this.attachmentContentMetadata != null
                               && this.attachmentContentMetadata.ContentType.StartsWith(ImageMimeType,
                                   StringComparison.OrdinalIgnoreCase);

        public bool IsVideo => this.attachmentContentMetadata != null
                               && this.attachmentContentMetadata.ContentType.StartsWith(VideoMimeType,
                                   StringComparison.OrdinalIgnoreCase);

        public bool IsAudio => this.attachmentContentMetadata != null
                               && this.attachmentContentMetadata.ContentType.StartsWith(AudioMimeType,
                                   StringComparison.OrdinalIgnoreCase);

        public bool IsPdf => this.attachmentContentMetadata != null
                             && this.attachmentContentMetadata.ContentType.StartsWith(PdfMimeType,
                                 StringComparison.OrdinalIgnoreCase);

        public IMvxCommand ShowPdf => new MvxCommand(OpenPdf);

        private void OpenPdf()
        {
            var interviewId = this.navigationState.InterviewId;
            pdfService.Open(interviewId, this.Identity);
        }

        public override void ViewDestroy(bool viewFinishing = true)
        {
            this.Video?.Release();
            this.Audio?.Release();
            this.ContentPath = null;
            base.ViewDestroy(viewFinishing);
        }

        public async Task HandleAsync(VariablesChanged @event)
        {
            await BindAttachment();
        }

        public void Dispose()
        {
            this.eventRegistry.Unsubscribe(this);
        }
    }
}
