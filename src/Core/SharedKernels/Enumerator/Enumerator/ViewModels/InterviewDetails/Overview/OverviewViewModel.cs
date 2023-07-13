using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MvvmCross;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Interview.Overview;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Overview
{
    public class OverviewViewModel : BaseViewModel
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IImageFileStorage fileStorage;
        private readonly IViewModelNavigationService navigationService;
        private readonly IAudioService audioService;
        private readonly IAudioFileStorage audioFileStorage;
        private readonly IUserInteractionService userInteractionService;
        private readonly IDynamicTextViewModelFactory dynamicTextViewModelFactory;
        private readonly DynamicTextViewModel nameViewModel;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IInterviewViewModelFactory interviewViewModelFactory;

        public OverviewViewModel(IStatefulInterviewRepository interviewRepository,
            IImageFileStorage fileStorage,
            IViewModelNavigationService navigationService,
            IAudioService audioService,
            IAudioFileStorage audioFileStorage,
            IUserInteractionService userInteractionService,
            IDynamicTextViewModelFactory dynamicTextViewModelFactory,
            DynamicTextViewModel nameViewModel,
            IQuestionnaireStorage questionnaireRepository,
            IInterviewViewModelFactory interviewViewModelFactory)
        {
            this.interviewRepository = interviewRepository;
            this.fileStorage = fileStorage;
            this.navigationService = navigationService;
            this.audioService = audioService;
            this.audioFileStorage = audioFileStorage;
            this.userInteractionService = userInteractionService;
            this.dynamicTextViewModelFactory = dynamicTextViewModelFactory;
            this.nameViewModel = nameViewModel;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewViewModelFactory = interviewViewModelFactory;
        }

        public void Configure(string interviewId, NavigationState navigationState)
        {
            this.InterviewId = interviewId;
            var interview = interviewRepository.Get(interviewId);
            var questionnaire = questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
            var sections = interview.GetEnabledSections()
                .Select(x => x.Identity)
                .ToImmutableHashSet();

            var identifyedEntities = questionnaire.GetPrefilledEntities()
                .Select(id => new Identity(id, RosterVector.Empty));
            var interviewEntities = identifyedEntities.Concat(interview.GetUnderlyingInterviewerEntities());

            this.Name = nameViewModel;
            this.Name.InitAsStatic(UIResources.Interview_Overview_Name);

            var coverIdentity = new Identity(questionnaire.CoverPageSectionId, RosterVector.Empty);
            var coverSectionItem = questionnaire.IsCoverPageSupported
                ? new OverviewSection(interview.GetGroup(coverIdentity))
                : OverviewSection.Empty(UIResources.Interview_Cover_Screen_Title);

            this.Items = new List<OverviewNode>() { coverSectionItem }
                .Concat(interviewEntities.Where(x => interview.IsEnabled(x)).Select(x => BuildOverviewNode(x, interview, questionnaire, sections, navigationState)))
                .ToList();
        }

        public string InterviewId { get; set; }

        private OverviewNode BuildOverviewNode(Identity interviewerEntityIdentity,
            IStatefulInterview interview,
            IQuestionnaire questionnaire,
            ICollection<Identity> sections, 
            NavigationState navigationState)
        {
            var question = interview.GetQuestion(interviewerEntityIdentity);

            if (question != null)
            {
                if (question.IsMultimedia)
                {
                    return new OverviewMultimediaQuestionViewModel(question, fileStorage, navigationService, userInteractionService, interview);
                }

                if (question.IsAudio)
                {
                    return new OverviewAudioQuestionViewModel(question, audioFileStorage, audioService, userInteractionService, interview);
                }

                if (question.IsSingleFixedOption || question.IsCascading)
                {
                    return new OverviewSingleCategoricalQuestionViewModel(question, interview, userInteractionService,
                        questionnaire, interviewViewModelFactory);
                }

                if (question.IsMultiFixedOption || question.IsYesNo)
                {
                    return new OverviewMultiCategoricalQuestionViewModel(question, interview, userInteractionService,
                        interviewViewModelFactory, questionnaire);
                }
                
                if (question.IsInteger || question.IsDouble)
                {
                    return new OverviewNumericQuestionViewModel(question, interview,userInteractionService,
                        questionnaire, interviewViewModelFactory);
                }

                return new OverviewQuestionViewModel(question, interview,userInteractionService);
            }

            var staticText = interview.GetStaticText(interviewerEntityIdentity);
            if (staticText != null)
            {
                return new OverviewStaticTextViewModel(staticText, Mvx.IoCProvider.Create<AttachmentViewModel>(), 
                    interview, 
                    userInteractionService,
                    navigationState)
                {
                    Id = staticText.Identity.ToString(),
                    Title = staticText.Title.Text
                };
            }

            var group = interview.GetGroup(interviewerEntityIdentity);
            if (group != null)
            {
                if (sections.Contains(group.Identity))
                {
                    return new OverviewSection(group)
                    {
                        Id = group.Identity.ToString(),
                        Title = group.Title.Text
                    };
                }

                var overviewGroupViewModel = new OverviewGroupViewModel(@group)
                {
                    Id = @group.Identity.ToString()
                };
                overviewGroupViewModel.Init(interview, dynamicTextViewModelFactory.CreateDynamicTextViewModel(), interviewerEntityIdentity);
                return overviewGroupViewModel;
            }
            
            var variable = interview.GetVariable(interviewerEntityIdentity);
            if (variable != null && questionnaire.IsPrefilled(interviewerEntityIdentity.Id))
            {
                return new OverviewVariableViewModel(variable, interview)
                {
                    Id = variable.Identity.ToString(),
                    Title = questionnaire.GetVariableLabel(variable.Identity.Id),
                };
            }

            throw new NotSupportedException($"Display of {interviewerEntityIdentity} entity is not supported");
        }

        public List<OverviewNode> Items { get; private set; }

        public DynamicTextViewModel Name { get; private set; }

        public override void Dispose()
        {
            audioService?.Dispose();
            nameViewModel?.Dispose();
            Name?.Dispose();

            foreach (var item in Items ?? new List<OverviewNode>())
            {
                item?.Dispose();
            }
            
            base.Dispose();
        }
    }
}

