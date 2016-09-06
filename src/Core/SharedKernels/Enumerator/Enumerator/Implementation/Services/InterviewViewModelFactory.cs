using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using MvvmCross.Platform;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using GroupViewModel = WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups.GroupViewModel;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    internal class InterviewViewModelFactory : IInterviewViewModelFactory
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
            
            MultiOptionQuestionModel = 180,
            LinkedMultiOptionQuestionModel = 181,
            YesNoQuestionModel = 182,
            LinkedToRosterMultiOptionQuestionModel = 183,
            
            GroupModel = 200,
            RosterModel = 201,
            StaticTextModel = 300
        }
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;

        private readonly Dictionary<InterviewEntityType, Func<IInterviewEntityViewModel>> EntityTypeToViewModelMap =
            new Dictionary<InterviewEntityType, Func<IInterviewEntityViewModel>>
            {
                { InterviewEntityType.StaticTextModel, Load<StaticTextViewModel> },
                { InterviewEntityType.IntegerNumericQuestionModel, Load<IntegerQuestionViewModel> },
                { InterviewEntityType.RealNumericQuestionModel, Load<RealQuestionViewModel> },
                { InterviewEntityType.TextQuestionModel, Load<TextQuestionViewModel> },
                { InterviewEntityType.TextListQuestionModel, Load<TextListQuestionViewModel> },
                { InterviewEntityType.SingleOptionQuestionModel, Load<SingleOptionQuestionViewModel> },
                { InterviewEntityType.LinkedSingleOptionQuestionModel, Load<SingleOptionLinkedQuestionViewModel> },
                { InterviewEntityType.LinkedToRosterSingleOptionQuestionModel, Load<SingleOptionRosterLinkedQuestionViewModel> },
                { InterviewEntityType.LinkedToRosterMultiOptionQuestionModel, Load<MultiOptionLinkedToRosterQuestionViewModel> },
                { InterviewEntityType.FilteredSingleOptionQuestionModel, Load<FilteredSingleOptionQuestionViewModel> },
                { InterviewEntityType.CascadingSingleOptionQuestionModel, Load<CascadingSingleOptionQuestionViewModel> },
                { InterviewEntityType.DateTimeQuestionModel, Load<DateTimeQuestionViewModel> },
                { InterviewEntityType.MultiOptionQuestionModel, Load<MultiOptionQuestionViewModel> },
                { InterviewEntityType.LinkedMultiOptionQuestionModel, Load<MultiOptionLinkedToQuestionQuestionViewModel> },
                { InterviewEntityType.GpsCoordinatesQuestionModel, Load<GpsCoordinatesQuestionViewModel> },
                { InterviewEntityType.MultimediaQuestionModel, Load<MultimedaQuestionViewModel> },
                { InterviewEntityType.QRBarcodeQuestionModel, Load<QRBarcodeQuestionViewModel> },
                { InterviewEntityType.YesNoQuestionModel, Load<YesNoQuestionViewModel> },
                { InterviewEntityType.GroupModel, Load<GroupViewModel> },
                { InterviewEntityType.RosterModel, Load<RosterViewModel>},
                { InterviewEntityType.TimestampQuestionModel, Load<TimestampQuestionViewModel>},
            };

        private static T Load<T>() where T : class => Mvx.Resolve<T>();

        public InterviewViewModelFactory(
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
        }

        public IEnumerable<IInterviewEntityViewModel> GetEntities(string interviewId, Identity groupIdentity, NavigationState navigationState)
        {
            if (groupIdentity == null) throw new ArgumentNullException(nameof(groupIdentity));

            var interviewEntityViewModels = this.GenerateViewModels(interviewId, groupIdentity, navigationState).ToList();
            return interviewEntityViewModels;
        }

        public IEnumerable<IInterviewEntityViewModel> GetPrefilledQuestions(string interviewId)
        {
            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            var tasks = questionnaire
                .GetPrefilledQuestions()
                .Select(questionId => this.CreateInterviewEntityViewModel(
                    identity: new Identity(questionId, RosterVector.Empty),
                    entityModelType: GetEntityModelType(questionId, questionnaire),
                    interviewId: interviewId,
                    navigationState: null));

            return tasks;
        }

        private IEnumerable<IInterviewEntityViewModel> GenerateViewModels(string interviewId, Identity groupIdentity, NavigationState navigationState)
        {
            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            if (!questionnaire.HasGroup(groupIdentity.Id))
                throw new KeyNotFoundException($"Questionnaire {interview.QuestionnaireIdentity} has no group with id {groupIdentity.Id}. Interview id: {interviewId}.");

            IReadOnlyList<Guid> groupWithoutNestedChildren = questionnaire.GetAllUnderlyingInterviewerEntities(groupIdentity.Id);

            IEnumerable<IInterviewEntityViewModel> viewmodels = groupWithoutNestedChildren.Select(questionnaireEntity => this.CreateInterviewEntityViewModel(
                identity: new Identity(questionnaireEntity, groupIdentity.RosterVector),
                entityModelType: GetEntityModelType(questionnaireEntity, questionnaire),
                interviewId: interviewId,
                navigationState: navigationState)).ToList();

            return viewmodels;
        }

        [Obsolete("Do not use it. It is for transition purpose only")]
        private static InterviewEntityType GetEntityModelType(Guid entityId, IQuestionnaire questionnaire)
        {
            if (questionnaire.HasGroup(entityId))
            {
                return questionnaire.IsRosterGroup(entityId) ? InterviewEntityType.RosterModel : InterviewEntityType.GroupModel;
            }
            if (questionnaire.HasQuestion(entityId))
            {
                var questionType = questionnaire.GetQuestionType(entityId);
                switch (questionType)
                {
                    case QuestionType.SingleOption:
                        if (questionnaire.IsQuestionLinked(entityId))
                        {
                            return InterviewEntityType.LinkedSingleOptionQuestionModel;
                        }
                        if (questionnaire.IsQuestionLinkedToRoster(entityId))
                        {
                            return InterviewEntityType.LinkedToRosterSingleOptionQuestionModel;
                        }
                        if (questionnaire.IsQuestionFilteredCombobox(entityId))
                        {
                            return InterviewEntityType.FilteredSingleOptionQuestionModel;
                        }
                        return questionnaire.IsQuestionCascading(entityId)
                            ? InterviewEntityType.CascadingSingleOptionQuestionModel
                            : InterviewEntityType.SingleOptionQuestionModel;

                    case QuestionType.MultyOption:
                        if (questionnaire.IsQuestionYesNo(entityId))
                        {
                            return InterviewEntityType.YesNoQuestionModel;
                        }
                        if (questionnaire.IsQuestionLinked(entityId))
                        {
                            return InterviewEntityType.LinkedMultiOptionQuestionModel;
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
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return InterviewEntityType.StaticTextModel;
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
    }
}