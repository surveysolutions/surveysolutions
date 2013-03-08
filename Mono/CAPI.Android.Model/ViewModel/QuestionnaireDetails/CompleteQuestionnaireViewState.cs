using System;
using System.Collections.Generic;

namespace CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails
{
    public class CompleteQuestionnaireViewState
    {
        public CompleteQuestionnaireViewState(Guid publicKey, string title, IDictionary<ItemPublicKey, IQuestionnaireViewModel> screens, IDictionary<Guid, QuestionnairePropagatedScreenViewModel> templates, IEnumerable<ItemPublicKey> chapters)
        {
            PublicKey = publicKey;
            Title = title;
            Screens = screens;
            Templates = templates;
            Chapters = chapters;
        }

        public Guid PublicKey { get; private set; }
        public string Title { get; private set; }
        public IEnumerable<ItemPublicKey> Chapters { get; private set; }
        public IDictionary<ItemPublicKey, IQuestionnaireViewModel> Screens { get; private set; }
        public IDictionary<Guid, QuestionnairePropagatedScreenViewModel> Templates { get; private set; }
    }
}