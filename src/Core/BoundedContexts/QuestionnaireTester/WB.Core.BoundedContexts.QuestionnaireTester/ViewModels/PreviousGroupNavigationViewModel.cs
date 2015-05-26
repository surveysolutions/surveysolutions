using System;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Properties;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;


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
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefullInterviewRepository interviewRepository;
        private readonly GroupStatisticsViewModel groupStatisticsViewModel;

        public PreviousGroupNavigationViewModel(
            ILiteEventRegistry liteEventRegistry,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository, 
            IStatefullInterviewRepository interviewRepository,
            GroupStatisticsViewModel groupStatisticsViewModel)
        {
            this.liteEventRegistry = liteEventRegistry;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.groupStatisticsViewModel = groupStatisticsViewModel;
        }


        public void Init(string interviewId, Identity groupIdentity, NavigationState navigationState)
        {
            this.interviewId = interviewId;
            this.entityIdentity = groupIdentity;
            this.navigationState = navigationState;

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);
            var patents = questionnaire.GroupParents[groupIdentity.Id];
            IsExistsParent = patents.Count > 0;

            if (IsExistsParent)
            {
                liteEventRegistry.Subscribe(this);
            }

            ButtonText = UIResources.Interview_PreviousGroupNavigation_ButtonText;

            UpdateSelfFromModel();
        }

        private string buttonText;
        public string ButtonText
        {
            get { return buttonText; }
            set { buttonText = value; RaisePropertyChanged(); }
        }

        private GroupStatistics statistics;
        public GroupStatistics Statistics
        {
            get { return statistics; }
            set { statistics = value; RaisePropertyChanged(); }
        }        
        
        private bool isExistsParent;
        public bool IsExistsParent
        {
            get { return this.isExistsParent; }
            set { this.isExistsParent = value; RaisePropertyChanged(); }
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
            Statistics = groupStatisticsViewModel.GetStatistics(interviewId, entityIdentity);
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