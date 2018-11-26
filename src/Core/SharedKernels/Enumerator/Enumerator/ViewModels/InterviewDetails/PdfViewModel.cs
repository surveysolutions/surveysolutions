using System.Threading.Tasks;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class PdfViewModel : MvxViewModel
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IAttachmentContentStorage attachmentContentStorage;
        private string attachmentPath;
        private Identity staticTextId;

        public PdfViewModel(IStatefulInterviewRepository interviewRepository,
            IQuestionnaireStorage questionnaireStorage,
            IAttachmentContentStorage attachmentContentStorage,
            DynamicTextViewModel dynamicTextViewModel)
        {
            this.interviewRepository = interviewRepository;
            this.questionnaireStorage = questionnaireStorage;
            this.attachmentContentStorage = attachmentContentStorage;
            this.Name = dynamicTextViewModel;
        }

        public DynamicTextViewModel Name { get; }

        public void Configure(string interviewId, Identity staticTextIdentity)
        {
            this.staticTextId = staticTextIdentity;

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
            var attachment = questionnaire.GetAttachmentForEntity(this.staticTextId.Id);

            var attachmentContentPath = this.attachmentContentStorage.GetFileCacheLocation(attachment.ContentId);
            this.AttachmentPath = attachmentContentPath;

            this.Name.Init(interviewId, staticTextIdentity);
        }

        public string AttachmentPath
        {
            get => attachmentPath;
            set => SetProperty(ref attachmentPath, value);
        }
    }

    public class PdfViewModelParameter
    {
        public string InterviewId { get; set; }
        public Identity StaticTextId { get; set; }
    }
}
