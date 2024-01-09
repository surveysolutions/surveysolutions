#nullable enable
using System;
using MvvmCross.Commands;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.ViewModels;

public abstract class PlayMediaViewModel: BaseViewModel<PlayMediaViewModelArgs>
{
    private readonly IAttachmentContentStorage attachmentContentStorage;
    private readonly IQuestionnaireStorage questionnaireRepository;
    private readonly IStatefulInterviewRepository interviewRepository;

    private PlayMediaViewModelArgs? initValues;
    
    public PlayMediaViewModel(
        IPrincipal principal,
        IViewModelNavigationService mvxNavigationService,
        IAttachmentContentStorage attachmentContentStorage, 
        IQuestionnaireStorage questionnaireRepository, 
        IStatefulInterviewRepository interviewRepository) 
        : base(principal, mvxNavigationService)
    {
        this.attachmentContentStorage = attachmentContentStorage;
        this.questionnaireRepository = questionnaireRepository;
        this.interviewRepository = interviewRepository;
    }

    public string? PathToFile { get; private set; }
    public string? Title { get; set; }
    protected abstract bool IsValidFileType(AttachmentContentMetadata? metadata);
    
    public override async void Prepare(PlayMediaViewModelArgs param)
    {
        base.Prepare();

        initValues = param;
        
        var interview = this.interviewRepository.GetOrThrow(param.InterviewId);
        IQuestionnaire questionnaire = this.questionnaireRepository.GetQuestionnaireOrThrow(interview.QuestionnaireIdentity, interview.Language);
        var attachment = questionnaire.GetAttachmentById(param.AttachmentId);
        
        var backingFile = await this.attachmentContentStorage.GetFileCacheLocationAsync(attachment.ContentId);

        if (!string.IsNullOrWhiteSpace(backingFile))
        {
            var attachmentContentMetadata = this.attachmentContentStorage.GetMetadata(attachment.ContentId);
            bool isValid = IsValidFileType(attachmentContentMetadata);
            if (isValid)
            {
                this.PathToFile = backingFile;
            }
        }
    }
    
    public IMvxAsyncCommand CloseCommand =>
        new MvxAsyncCommand(async () => await ViewModelNavigationService.Close(this));
}
