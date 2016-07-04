﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform.Core;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public abstract class EnumeratorInterviewViewModel : BaseViewModel, IDisposable
    {
        private readonly IPlainQuestionnaireRepository questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        protected readonly NavigationState navigationState;
        private readonly AnswerNotifier answerNotifier;
        private readonly IAnswerToStringService answerToStringService;
        private readonly GroupStateViewModel groupState;
        private readonly InterviewStateViewModel interviewState;
        protected readonly IInterviewViewModelFactory interviewViewModelFactory;
        protected string interviewId;
        private IStatefulInterview interview;


        protected EnumeratorInterviewViewModel(
            IPlainQuestionnaireRepository questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            IAnswerToStringService answerToStringService,
            SideBarSectionsViewModel sectionsViewModel, 
            BreadCrumbsViewModel breadCrumbsViewModel,
            NavigationState navigationState,
            AnswerNotifier answerNotifier,
            GroupStateViewModel groupState, 
            InterviewStateViewModel interviewState,
            IPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            IInterviewViewModelFactory interviewViewModelFactory) : base(principal, viewModelNavigationService)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.navigationState = navigationState;
            this.answerNotifier = answerNotifier;
            this.answerToStringService = answerToStringService;
            this.groupState = groupState;
            this.interviewState = interviewState;
            this.interviewViewModelFactory = interviewViewModelFactory;

            this.BreadCrumbs = breadCrumbsViewModel;
            this.Sections = sectionsViewModel;
        }

        public void Init(string interviewId)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            this.interviewId = interviewId;
        }

        public override void Load()
        {
            if (this.interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            interview = this.interviewRepository.Get(interviewId);
            if (interview == null) throw new Exception("Interview is null.");
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity);
            if (questionnaire == null) throw new Exception("Questionnaire not found. QuestionnaireId: " + interview.QuestionnaireId);

            this.QuestionnaireTitle = questionnaire.Title;
            this.PrefilledQuestions = questionnaire
                .GetPrefilledQuestions()
                .Select(questionId => new SideBarPrefillQuestion
                {
                    Question = questionnaire.GetQuestionTitle(questionId),
                    Answer = this.GetAnswer(questionnaire, questionId),
                    StatsInvisible = questionnaire.GetQuestionType(questionId) == QuestionType.GpsCoordinates,
                })
                .ToList();

            this.BreadCrumbs.Init(interviewId, this.navigationState);
            this.Sections.Init(interviewId, interview.QuestionnaireIdentity, this.navigationState);

            this.navigationState.Init(interviewId: interviewId, questionnaireId: interview.QuestionnaireId);
            this.navigationState.ScreenChanged += this.OnScreenChanged;
            this.navigationState.NavigateTo(NavigationIdentity.CreateForGroup(new Identity(questionnaire.GetAllSections().First(), new decimal[0])));

            this.answerNotifier.QuestionAnswered += this.AnswerNotifierOnQuestionAnswered;
        }

        private void AnswerNotifierOnQuestionAnswered(object sender, EventArgs eventArgs)
        {
            if (this.navigationState.CurrentScreenType == ScreenType.Group)
            {
                this.UpdateGroupStatus(this.navigationState.CurrentGroup);
            }
        }

        private void OnScreenChanged(ScreenChangedEventArgs eventArgs)
        {
            if (eventArgs.TargetScreen != ScreenType.Group)
            {
                this.UpdateInterviewStatus(null);
            }
            else
            {
                IEnumerable<Identity> questionsToListen = interview.GetChildQuestions(eventArgs.TargetGroup);

                this.answerNotifier.Init(this.interviewId, questionsToListen.ToArray());

                this.UpdateGroupStatus(eventArgs.TargetGroup);
            }

            this.CurrentStage.DisposeIfDisposable();
            this.CurrentStage = this.UpdateCurrentScreenViewModel(eventArgs);
            this.RaisePropertyChanged(() => this.CurrentStage);
        }

        protected virtual MvxViewModel UpdateCurrentScreenViewModel(ScreenChangedEventArgs eventArgs)
        {
            if (this.navigationState.CurrentScreenType == ScreenType.Complete)
            {
                var completeInterviewViewModel = interviewViewModelFactory.GetNew<CompleteInterviewViewModel>();
                completeInterviewViewModel.Init(this.interviewId, this.navigationState);
                return completeInterviewViewModel;
            }
            else
            {
                var activeStageViewModel = interviewViewModelFactory.GetNew<EnumerationStageViewModel>();
                activeStageViewModel.Init(interviewId, this.navigationState, eventArgs.TargetGroup, eventArgs.AnchoredElementIdentity);
                return activeStageViewModel;
            }
        }

        private void UpdateGroupStatus(Identity groupIdentity)
        {
            this.groupState.Init(this.navigationState.InterviewId, groupIdentity);
            this.Status = this.groupState.Status;
        }

        private void UpdateInterviewStatus(Identity groupIdentity)
        {
            this.interviewState.Init(this.navigationState.InterviewId, groupIdentity);
            this.Status = this.interviewState.Status;
        }

        private string GetAnswer(IQuestionnaire questionnaire, Guid referenceToQuestionId)
        {
            var identityAsString = ConversionHelper.ConvertIdAndRosterVectorToString(referenceToQuestionId, new decimal[0]);
            var interviewAnswer = interview.Answers.ContainsKey(identityAsString) ? interview.Answers[identityAsString] : null;
            return this.answerToStringService.AnswerToUIString(referenceToQuestionId, interviewAnswer, interview, questionnaire);
        }

        private GroupStatus status;

        public GroupStatus Status
        {
            get { return this.status; }
            private set
            {
                if (this.status != value)
                {
                    this.status = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public BreadCrumbsViewModel BreadCrumbs { get; set; }
        public SideBarSectionsViewModel Sections { get; set; }
        public string QuestionnaireTitle { get; set; }
        public IEnumerable<SideBarPrefillQuestion> PrefilledQuestions { get; set; }

        public MvxViewModel CurrentStage { get; private set; }
        public string Title { get; private set; }

        public IEnumerable<SideBarPrefillQuestion> PrefilledQuestionsStats
        {
            get { return PrefilledQuestions.Where(x => !x.StatsInvisible); }
        }

        public void Dispose()
        {
            this.navigationState.ScreenChanged -= this.OnScreenChanged;
            this.answerNotifier.QuestionAnswered -= this.AnswerNotifierOnQuestionAnswered;
            this.CurrentStage.DisposeIfDisposable();
            this.answerNotifier.Dispose();
            this.BreadCrumbs.Dispose();
            this.Sections.Dispose();
        }
    }
}