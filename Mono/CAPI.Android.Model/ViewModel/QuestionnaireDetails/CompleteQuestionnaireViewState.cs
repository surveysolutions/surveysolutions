using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails
{
    public class CompleteQuestionnaireViewState
    {
        public CompleteQuestionnaireViewState(Guid publicKey, string title, IDictionary<InterviewItemId, IQuestionnaireViewModel> screens, IDictionary<Guid, QuestionnairePropagatedScreenViewModel> templates, IEnumerable<InterviewItemId> chapters)
        {
            PublicKey = publicKey;
            Title = title;
            Screens = screens;
            Templates = templates;
            Chapters = chapters;
        }

        public Guid PublicKey { get; private set; }
        public string Title { get; private set; }
        public IEnumerable<InterviewItemId> Chapters { get; private set; }
        public IDictionary<InterviewItemId, IQuestionnaireViewModel> Screens { get; private set; }
        public IDictionary<Guid, QuestionnairePropagatedScreenViewModel> Templates { get; private set; }
    }
}