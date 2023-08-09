using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Enumerator.Views.Dashboard;
using GroupViewModel = WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups.GroupViewModel;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public abstract class InterviewViewModelFactory : IInterviewViewModelFactory
    {
        private enum InterviewEntityType
        {
            IntegerNumericQuestionModel = 100,
            RealNumericQuestionModel = 101,

            DateTimeQuestionModel = 110,
            TimestampQuestionModel = 111,

            TextQuestionModel = 120,
            TextListQuestionModel = 130,
            GpsCoordinatesQuestionModel = 140,
            MultimediaQuestionModel = 150,
            QRBarcodeQuestionModel = 160,
            
            SingleOptionQuestionModel = 170,
            LinkedSingleOptionQuestionModel = 171,
            LinkedToRosterSingleOptionQuestionModel = 172,
            FilteredSingleOptionQuestionModel = 173,
            CascadingSingleOptionQuestionModel = 174,

            MultiOptionComboboxQuestionModel = 179,
            MultiOptionQuestionModel = 180,
            LinkedMultiOptionQuestionModel = 181,
            YesNoQuestionModel = 182,
            LinkedToRosterMultiOptionQuestionModel = 183,

            LinkedToListQuestionMultiOptionQuestionModel = 184,
            LinkedToListQuestionSingleOptionQuestionModel = 185,

            AudioQuestionModel = 186,

            AreaQuestionModel = 190,

            GroupModel = 200,
            RosterModel = 201,
            StaticTextModel = 300,
            VariableModel = 400,
            ReadOnlyQuestion = 500,
            FlatRoster = 600,
            AutocompleteLinkedSingleOptionQuestionModel = 700
        }
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IEnumeratorSettings settings;
        protected readonly IServiceLocator ServiceLocator;


        private Dictionary<InterviewEntityType, Func<IInterviewEntityViewModel>> entityTypeToViewModelMap;
        private Dictionary<InterviewEntityType, Func<IInterviewEntityViewModel>> EntityTypeToViewModelMap
        {
            get
            {
                if (entityTypeToViewModelMap == null)
                {
                    this.entityTypeToViewModelMap = new Dictionary<InterviewEntityType, Func<IInterviewEntityViewModel>>
                    {
                        {InterviewEntityType.StaticTextModel, Load<StaticTextViewModel>},
                        {InterviewEntityType.IntegerNumericQuestionModel, Load<IntegerQuestionViewModel>},
                        {InterviewEntityType.RealNumericQuestionModel, Load<RealQuestionViewModel>},
                        {InterviewEntityType.TextQuestionModel, Load<TextQuestionViewModel>},
                        {InterviewEntityType.TextListQuestionModel, Load<TextListQuestionViewModel>},
                        {InterviewEntityType.SingleOptionQuestionModel, Load<SingleOptionQuestionViewModel>},
                        {
                            InterviewEntityType.LinkedSingleOptionQuestionModel,
                            Load<SingleOptionLinkedQuestionViewModel>
                        },
                        {
                            InterviewEntityType.LinkedToRosterSingleOptionQuestionModel,
                            Load<SingleOptionRosterLinkedQuestionViewModel>
                        },
                        {
                            InterviewEntityType.AutocompleteLinkedSingleOptionQuestionModel,
                            Load<AutoCompleteSingleOptionLinkedQuestionViewModel>
                        },
                        {
                            InterviewEntityType.LinkedToListQuestionSingleOptionQuestionModel,
                            Load<SingleOptionLinkedToListQuestionViewModel>
                        },
                        {
                            InterviewEntityType.LinkedToRosterMultiOptionQuestionModel,
                            Load<CategoricalMultiLinkedToRosterTitleViewModel>
                        },
                        {
                            InterviewEntityType.LinkedToListQuestionMultiOptionQuestionModel,
                            Load<CategoricalMultiLinkedToListViewModel>
                        },
                        {
                            InterviewEntityType.FilteredSingleOptionQuestionModel,
                            Load<FilteredSingleOptionQuestionViewModel>
                        },
                        {
                            InterviewEntityType.CascadingSingleOptionQuestionModel,
                            Load<CascadingSingleOptionQuestionViewModel>
                        },
                        {InterviewEntityType.DateTimeQuestionModel, Load<DateTimeQuestionViewModel>},
                        {InterviewEntityType.MultiOptionQuestionModel, Load<CategoricalMultiViewModel>},
                        {InterviewEntityType.MultiOptionComboboxQuestionModel, Load<CategoricalMultiComboboxViewModel>},
                        {
                            InterviewEntityType.LinkedMultiOptionQuestionModel,
                            Load<CategoricalMultiLinkedToQuestionViewModel>
                        },
                        {InterviewEntityType.GpsCoordinatesQuestionModel, Load<GpsCoordinatesQuestionViewModel>},
                        {InterviewEntityType.MultimediaQuestionModel, Load<MultimediaQuestionViewModel>},
                        {InterviewEntityType.QRBarcodeQuestionModel, Load<QRBarcodeQuestionViewModel>},
                        {InterviewEntityType.YesNoQuestionModel, Load<CategoricalYesNoViewModel>},
                        {InterviewEntityType.GroupModel, Load<GroupViewModel>},
                        {InterviewEntityType.RosterModel, Load<RosterViewModel>},
                        {InterviewEntityType.TimestampQuestionModel, Load<TimestampQuestionViewModel>},
                        {InterviewEntityType.VariableModel, Load<VariableViewModel>},
                        {InterviewEntityType.ReadOnlyQuestion, Load<ReadOnlyQuestionViewModel>},
                        {InterviewEntityType.AreaQuestionModel, Load<AreaQuestionViewModel>},
                        {InterviewEntityType.AudioQuestionModel, Load<AudioQuestionViewModel>},
                        {InterviewEntityType.FlatRoster, Load<FlatRosterViewModel>},
                    };
                }

                return entityTypeToViewModelMap;
            }
        }

        private T Load<T>() where T : class => this.ServiceLocator.GetInstance<T>();

        public InterviewViewModelFactory(IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            IEnumeratorSettings settings, 
            IServiceLocator serviceLocator)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.settings = settings;
            this.ServiceLocator = serviceLocator;
        }

        public List<IInterviewEntityViewModel> GetEntities(string interviewId, Identity groupIdentity, NavigationState navigationState)
        {
            if (groupIdentity == null) throw new ArgumentNullException(nameof(groupIdentity));

            var interviewEntityViewModels = this.GenerateViewModels(interviewId, groupIdentity, navigationState);
            return interviewEntityViewModels;
        }

        public IEnumerable<IInterviewEntityViewModel> GetPrefilledEntities(string interviewId, NavigationState navigationState)
        {
            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            var tasks = questionnaire
                .GetPrefilledEntities()
                .Select(questionId => this.CreateInterviewEntityViewModel(
                    identity: new Identity(questionId, RosterVector.Empty),
                    entityModelType: GetEntityModelType(new Identity(questionId, RosterVector.Empty), questionnaire, interview),
                    interviewId: interviewId,
                    navigationState: navigationState));

            return tasks;
        }

        private List<IInterviewEntityViewModel> GenerateViewModels(string interviewId, Identity groupIdentity, NavigationState navigationState)
        {
            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            if (!questionnaire.HasGroup(groupIdentity.Id))
                throw new KeyNotFoundException($"Questionnaire {interview.QuestionnaireIdentity} has no group with id {groupIdentity.Id}. Interview id: {interviewId}.");

            IReadOnlyList<Guid> groupWithoutNestedChildren = GetUnderlyingInterviewerEntities(groupIdentity, questionnaire);

            List<IInterviewEntityViewModel> viewmodels = groupWithoutNestedChildren
                .Where(entityId => this.settings.ShowVariables || !questionnaire.HasVariable(entityId))
                .Select(questionnaireEntity => this.CreateInterviewEntityViewModel(
                    identity: new Identity(questionnaireEntity, groupIdentity.RosterVector),
                    entityModelType: GetEntityModelType(new Identity(questionnaireEntity, groupIdentity.RosterVector), questionnaire, interview),
                    interviewId: interviewId,
                    navigationState: navigationState))
                .ToList();

            return viewmodels;
        }

        public virtual IReadOnlyList<Guid> GetUnderlyingInterviewerEntities(Identity groupIdentity, IQuestionnaire questionnaire)
        {
            return questionnaire.GetAllUnderlyingInterviewerEntities(groupIdentity.Id);
        }

        private static InterviewEntityType GetEntityModelType(Identity identity, IQuestionnaire questionnaire, IStatefulInterview interview)
        {
            Guid entityId = identity.Id;

            if (questionnaire.HasGroup(entityId))
            {
                if (questionnaire.IsRosterGroup(entityId))
                    return questionnaire.IsFlatRoster(entityId) ? InterviewEntityType.FlatRoster : InterviewEntityType.RosterModel;
                else
                    return InterviewEntityType.GroupModel;
            }

            if (questionnaire.HasQuestion(entityId))
            {
                var questionType = questionnaire.GetQuestionType(entityId);

                if (interview.IsReadOnlyQuestion(identity))
                {
                    if (questionType == QuestionType.GpsCoordinates)
                        return InterviewEntityType.GpsCoordinatesQuestionModel;
                    return InterviewEntityType.ReadOnlyQuestion;
                }

                switch (questionType)
                {
                    case QuestionType.SingleOption:
                        if (questionnaire.IsQuestionLinked(entityId))
                        {
                            if (questionnaire.IsLinkedToListQuestion(entityId))
                                return InterviewEntityType.LinkedToListQuestionSingleOptionQuestionModel;
                            
                            if(questionnaire.IsQuestionFilteredCombobox(entityId))
                                return InterviewEntityType.AutocompleteLinkedSingleOptionQuestionModel;
                            return InterviewEntityType.LinkedSingleOptionQuestionModel;
                        }

                        if (questionnaire.IsQuestionLinkedToRoster(entityId))
                        {
                            if (questionnaire.IsQuestionFilteredCombobox(entityId))
                                return InterviewEntityType.AutocompleteLinkedSingleOptionQuestionModel;
                            
                            return InterviewEntityType.LinkedToRosterSingleOptionQuestionModel;
                        }

                        if (questionnaire.IsQuestionCascading(entityId))
                            return InterviewEntityType.CascadingSingleOptionQuestionModel;

                        if (questionnaire.IsQuestionFilteredCombobox(entityId))
                            return InterviewEntityType.FilteredSingleOptionQuestionModel;

                        return InterviewEntityType.SingleOptionQuestionModel;
                    case QuestionType.MultyOption:
                        if (questionnaire.IsQuestionYesNo(entityId))
                            return InterviewEntityType.YesNoQuestionModel;

                        if (questionnaire.IsQuestionFilteredCombobox(entityId))
                            return InterviewEntityType.MultiOptionComboboxQuestionModel;

                        if (questionnaire.IsQuestionLinked(entityId))
                        {
                            return questionnaire.IsLinkedToListQuestion(entityId)
                                ? InterviewEntityType.LinkedToListQuestionMultiOptionQuestionModel
                                : InterviewEntityType.LinkedMultiOptionQuestionModel;
                        }
                        return questionnaire.IsQuestionLinkedToRoster(entityId)
                            ? InterviewEntityType.LinkedToRosterMultiOptionQuestionModel
                            : InterviewEntityType.MultiOptionQuestionModel;
                    case QuestionType.Numeric:
                        return questionnaire.IsQuestionInteger(entityId)
                            ? InterviewEntityType.IntegerNumericQuestionModel
                            : InterviewEntityType.RealNumericQuestionModel;
                    case QuestionType.DateTime:
                        return questionnaire.IsTimestampQuestion(entityId)
                            ? InterviewEntityType.TimestampQuestionModel
                            : InterviewEntityType.DateTimeQuestionModel;
                    case QuestionType.GpsCoordinates:
                        return InterviewEntityType.GpsCoordinatesQuestionModel;
                    case QuestionType.Text:
                        return InterviewEntityType.TextQuestionModel;
                    case QuestionType.TextList:
                        return InterviewEntityType.TextListQuestionModel;
                    case QuestionType.QRBarcode:
                        return InterviewEntityType.QRBarcodeQuestionModel;
                    case QuestionType.Multimedia:
                        return InterviewEntityType.MultimediaQuestionModel;
                    case QuestionType.Area:
                        return InterviewEntityType.AreaQuestionModel;
                    case QuestionType.Audio:
                        return InterviewEntityType.AudioQuestionModel;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            if (questionnaire.HasVariable(entityId))
            {
                return InterviewEntityType.VariableModel;
            }
            if (questionnaire.HasStaticText(entityId))
            {
                return InterviewEntityType.StaticTextModel;
            }

            throw new ArgumentException("Don't found type for entity : " + entityId);
        }

        public IInterviewEntityViewModel GetEntity(Identity identity,
            string interviewId, 
            NavigationState navigationState)
        {
            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire =
                this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            var modelType = GetEntityModelType(identity, questionnaire, interview);

            return CreateInterviewEntityViewModel(identity, modelType, interviewId, navigationState);
        }

        private IInterviewEntityViewModel CreateInterviewEntityViewModel(
            Identity identity,
            InterviewEntityType entityModelType,
            string interviewId,
            NavigationState navigationState)
        {
            if (!this.EntityTypeToViewModelMap.ContainsKey(entityModelType))
            {
                var text = (StaticTextViewModel)this.EntityTypeToViewModelMap[InterviewEntityType.StaticTextModel].Invoke();
                text.Text.PlainText = entityModelType.ToString();
                return text;
            }

            Func<IInterviewEntityViewModel> viewModelActivator = this.EntityTypeToViewModelMap[entityModelType];

            IInterviewEntityViewModel viewModel = viewModelActivator.Invoke();
            viewModel.Init(interviewId: interviewId, entityIdentity: identity, navigationState: navigationState);
            return viewModel;
        }

        public T GetNew<T>() where T : class
        {
            return Load<T>();
        }

        public abstract IDashboardItem GetDashboardAssignment(AssignmentDocument assignment);

        public virtual IDashboardItem GetDashboardInterview(InterviewView interviewView, List<PrefilledQuestion> details)
        {
            var result = GetNew<InterviewDashboardItemViewModel>();
            result.Init(interviewView, details);
            return result;
        }
    }
}
