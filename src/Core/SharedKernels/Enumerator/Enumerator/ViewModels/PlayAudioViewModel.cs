#nullable enable
using System;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.ViewModels;

public class PlayAudioViewModel: PlayMediaViewModel
{
    private const string AudioMimeType = "audio/";

    public PlayAudioViewModel(IPrincipal principal, 
        IViewModelNavigationService mvxNavigationService, 
        IAttachmentContentStorage attachmentContentStorage, 
        IQuestionnaireStorage questionnaireRepository, 
        IStatefulInterviewRepository interviewRepository) 
        : base(principal, mvxNavigationService, attachmentContentStorage, questionnaireRepository, interviewRepository)
    {
    }

    protected override bool IsValidFileType(AttachmentContentMetadata? metadata)
    {
        bool isAudio = metadata != null
                       && metadata.ContentType.StartsWith(AudioMimeType,
                           StringComparison.OrdinalIgnoreCase);
        return isAudio;
    }
}
