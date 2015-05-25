using System;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;


namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class PreviousGroupNavigationViewModel : MvxNotifyPropertyChanged, 
        IInterviewEntityViewModel,
        ILiteEventHandler<TextQuestionAnswered>,
        ILiteEventHandler<TextListQuestionAnswered>,
        ILiteEventHandler<SingleOptionLinkedQuestionAnswered>,
        ILiteEventHandler<SingleOptionQuestionAnswered>,
        ILiteEventHandler<QRBarcodeQuestionAnswered>,
        ILiteEventHandler<PictureQuestionAnswered>,
        ILiteEventHandler<NumericIntegerQuestionAnswered>,
        ILiteEventHandler<NumericRealQuestionAnswered>,
        ILiteEventHandler<MultipleOptionsLinkedQuestionAnswered>,
        ILiteEventHandler<MultipleOptionsQuestionAnswered>,
        ILiteEventHandler<GeoLocationQuestionAnswered>,
        ILiteEventHandler<DateTimeQuestionAnswered>

    {
        private string interviewId;
        private Identity entityIdentity;
        private NavigationState navigationState;

        private readonly ILiteEventRegistry liteEventRegistry;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> plainQuestionnaireRepository;
        private readonly IStatefullInterviewRepository interviewRepository;
        private readonly GroupStatisticsViewModel groupStatisticsViewModel;

        public PreviousGroupNavigationViewModel(
            ILiteEventRegistry liteEventRegistry,
            IPlainKeyValueStorage<QuestionnaireModel> plainQuestionnaireRepository, 
            IStatefullInterviewRepository interviewRepository,
            GroupStatisticsViewModel groupStatisticsViewModel)
        {
            this.liteEventRegistry = liteEventRegistry;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.groupStatisticsViewModel = groupStatisticsViewModel;
        }


        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.interviewId = interviewId;
            this.entityIdentity = entityIdentity;
            this.navigationState = navigationState;

            liteEventRegistry.Subscribe(this);

            UpdateSelfFromModel();
        }

        private string buttonText;
        public string ButtonText
        {
            get { return buttonText; }
            set { buttonText = value; RaisePropertyChanged(); }
        }

        private string note;
        public string Note
        {
            get { return note; }
            set { note = value; RaisePropertyChanged(); }
        }

        private IMvxCommand navigateToRosterCommand;
        public IMvxCommand NavigateToRosterCommand
        {
            get { return navigateToRosterCommand ?? (navigateToRosterCommand = new MvxCommand(SendAnswerTextQuestionCommand)); }
        }

        private void SendAnswerTextQuestionCommand()
        {
            navigationState.NavigateBack(() => { });
        }

        private void UpdateSelfFromModel()
        {
            var statistics = groupStatisticsViewModel.GetStatistics(interviewId, entityIdentity);

            ButtonText = "Back to&#10;parent group";

            Note = statistics.UnansweredQuestionsCount > 0
                ? "Note: {0} questions is unanswered".FormatString(statistics.UnansweredQuestionsCount)
                : "All {0} questions answered".FormatString(statistics.AnsweredQuestionsCount);
        }

        #region Handle All Answers events

        public void Handle(TextQuestionAnswered @event)
        {
            UpdateSelfFromModel();
        }

        public void Handle(TextListQuestionAnswered @event)
        {
            UpdateSelfFromModel();
        }

        public void Handle(SingleOptionLinkedQuestionAnswered @event)
        {
            UpdateSelfFromModel();
        }

        public void Handle(SingleOptionQuestionAnswered @event)
        {
            UpdateSelfFromModel();
        }

        public void Handle(QRBarcodeQuestionAnswered @event)
        {
            UpdateSelfFromModel();
        }

        public void Handle(PictureQuestionAnswered @event)
        {
            UpdateSelfFromModel();
        }

        public void Handle(NumericIntegerQuestionAnswered @event)
        {
            UpdateSelfFromModel();
        }

        public void Handle(NumericRealQuestionAnswered @event)
        {
            UpdateSelfFromModel();
        }

        public void Handle(MultipleOptionsLinkedQuestionAnswered @event)
        {
            UpdateSelfFromModel();
        }

        public void Handle(MultipleOptionsQuestionAnswered @event)
        {
            UpdateSelfFromModel();
        }

        public void Handle(GeoLocationQuestionAnswered @event)
        {
            UpdateSelfFromModel();
        }

        public void Handle(DateTimeQuestionAnswered @event)
        {
            UpdateSelfFromModel();
        }

        #endregion
    }
}