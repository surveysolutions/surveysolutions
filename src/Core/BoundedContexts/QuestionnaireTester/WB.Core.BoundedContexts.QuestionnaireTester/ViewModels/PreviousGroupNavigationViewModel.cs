using System;
using System.Collections.Generic;
using System.Linq;
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
        private Identity groupIdentity;
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
            this.groupIdentity = groupIdentity;
            this.navigationState = navigationState;

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);
            var parents = questionnaire.Parents[groupIdentity.Id];
            IsExistsParent = parents.Count > 0;

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
            var parentIdentity = GetParentIdentity();
            navigationState.NavigateTo(parentIdentity);
        }

        private Identity GetParentIdentity()
        {
            var interview = this.interviewRepository.Get(this.interviewId);
            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);

            var parents = questionnaire.Parents[groupIdentity.Id];
            var parent = parents.Last();
            int rosterLevelOfParent = parents.Count(p => p.IsRoster);
            decimal[] parentRosterVector = groupIdentity.RosterVector.Take(rosterLevelOfParent).ToArray();
            return new Identity(parent.Id, parentRosterVector);
        }

        private void UpdateSelfFromModel()
        {
            Statistics = groupStatisticsViewModel.GetStatistics(interviewId, this.groupIdentity);
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