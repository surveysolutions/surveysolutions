using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using MvvmCross.Platform;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using GroupViewModel = WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups.GroupViewModel;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    internal class InterviewViewModelFactory : IInterviewViewModelFactory
    {
        private readonly IPlainQuestionnaireRepository questionnaireRepository;
        private readonly IPlainQuestionnaireRepository questionnaireModelRepository;
        private readonly IStatefulInterviewRepository interviewRepository;

        private readonly Dictionary<Type, Func<IInterviewEntityViewModel>> EntityTypeToViewModelMap =
            new Dictionary<Type, Func<IInterviewEntityViewModel>>
            {
                { typeof(StaticTextModel), Load<StaticTextViewModel> },
                { typeof(IntegerNumericQuestionModel), Load<IntegerQuestionViewModel> },
                { typeof(RealNumericQuestionModel), Load<RealQuestionViewModel> },
                { typeof(TextQuestionModel), Load<TextQuestionViewModel> },
                { typeof(TextListQuestionModel), Load<TextListQuestionViewModel> },
                { typeof(SingleOptionQuestionModel), Load<SingleOptionQuestionViewModel> },
                { typeof(LinkedSingleOptionQuestionModel), Load<SingleOptionLinkedQuestionViewModel> },
                { typeof(LinkedToRosterSingleOptionQuestionModel), Load<SingleOptionRosterLinkedQuestionViewModel> },
                { typeof(LinkedToRosterMultiOptionQuestionModel), Load<MultiOptionLinkedToRosterQuestionViewModel> },
                { typeof(FilteredSingleOptionQuestionModel), Load<FilteredSingleOptionQuestionViewModel> },
                { typeof(CascadingSingleOptionQuestionModel), Load<CascadingSingleOptionQuestionViewModel> },
                { typeof(DateTimeQuestionModel), Load<DateTimeQuestionViewModel> },
                { typeof(MultiOptionQuestionModel), Load<MultiOptionQuestionViewModel> },
                { typeof(LinkedMultiOptionQuestionModel), Load<MultiOptionLinkedToQuestionQuestionViewModel> },
                { typeof(GpsCoordinatesQuestionModel), Load<GpsCoordinatesQuestionViewModel> },
                { typeof(MultimediaQuestionModel), Load<MultimedaQuestionViewModel> },
                { typeof(QRBarcodeQuestionModel), Load<QRBarcodeQuestionViewModel> },
                { typeof(YesNoQuestionModel), Load<YesNoQuestionViewModel> },
                { typeof(GroupModel), Load<GroupViewModel> },
                { typeof(RosterModel), Load<GroupViewModel>}
            };

        private static T Load<T>() where T : class
        {
            return Mvx.Resolve<T>();
        }

        public InterviewViewModelFactory(
            IPlainQuestionnaireRepository questionnaireRepository,
            IPlainQuestionnaireRepository questionnaireModelRepository,
            IStatefulInterviewRepository interviewRepository)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.questionnaireModelRepository = questionnaireModelRepository;
            this.interviewRepository = interviewRepository;
        }

        public IEnumerable<IInterviewEntityViewModel> GetEntities(string interviewId, Identity groupIdentity, NavigationState navigationState)
        {
            if (groupIdentity == null) throw new ArgumentNullException(nameof(groupIdentity));

            return this.GenerateViewModels(interviewId, groupIdentity, navigationState);
        }

        public IEnumerable<IInterviewEntityViewModel> GetPrefilledQuestions(string interviewId) => this.GetPrefilledQuestionsImpl(interviewId);

        private IEnumerable<IInterviewEntityViewModel> GenerateViewModels(string interviewId, Identity groupIdentity, NavigationState navigationState)
        {
            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireModelRepository.GetQuestionnaire(interview.QuestionnaireIdentity);

            if (!questionnaire.HasGroup(groupIdentity.Id))
                throw new KeyNotFoundException($"Group with id {groupIdentity.Id.FormatGuid()} don't found");

            var groupWithoutNestedChildren = interview.GetInterviewerEntities(groupIdentity);

            return groupWithoutNestedChildren.Select(questionnaireEntity => this.CreateInterviewEntityViewModel(
                entityId: questionnaireEntity.Id,
                rosterVector: questionnaireEntity.RosterVector,
                entityModelType: GetQuestionModelType(questionnaireEntity.Id, questionnaire),
                interviewId: interviewId,
                navigationState: navigationState));
        }

        [Obsolete("Do not use it. It is for transition purpose only")]
        private static Type GetQuestionModelType(Guid questionId, IQuestionnaire questionnaire)
        {
            var questionType = questionnaire.GetQuestionType(questionId);
            switch (questionType)
            {
                case QuestionType.SingleOption:
                    if (questionnaire.IsQuestionLinked(questionId))
                    {
                        return typeof (LinkedSingleOptionQuestionModel);
                    }
                    if (questionnaire.IsQuestionLinkedToRoster(questionId))
                    {
                        return typeof(LinkedToRosterSingleOptionQuestionModel);
                    }
                    if (questionnaire.IsQuestionFilteredCombobox(questionId))
                    {
                        return typeof(FilteredSingleOptionQuestionModel);
                    }
                    return questionnaire.IsQuestionCascading(questionId)
                        ? typeof(CascadingSingleOptionQuestionModel) 
                        : typeof(SingleOptionQuestionModel);

                case QuestionType.MultyOption:
                    if (questionnaire.IsQuestionYesNo(questionId))
                    {
                        return typeof(YesNoQuestionModel);
                    }
                    if (questionnaire.IsQuestionLinked(questionId))
                    {
                        return typeof(LinkedMultiOptionQuestionModel);
                    }
                    return questionnaire.IsQuestionLinkedToRoster(questionId)
                        ? typeof(LinkedToRosterMultiOptionQuestionModel) 
                        : typeof(MultiOptionQuestionModel);
                case QuestionType.Numeric:
                    return questionnaire.IsQuestionInteger(questionId)
                        ? typeof(IntegerNumericQuestionModel) 
                        : typeof(RealNumericQuestionModel);
                case QuestionType.DateTime:
                    return typeof(DateTimeQuestionModel);
                case QuestionType.GpsCoordinates:
                    return typeof(GpsCoordinatesQuestionModel);
                case QuestionType.Text:
                    return typeof(TextQuestionModel);
                case QuestionType.TextList:
                    return typeof(TextListQuestionModel);
                case QuestionType.QRBarcode:
                    return typeof(QRBarcodeQuestionModel);
                case QuestionType.Multimedia:
                    return typeof(MultimediaQuestionModel);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IEnumerable<IInterviewEntityViewModel> GetPrefilledQuestionsImpl(string interviewId)
        {
            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity);

            return questionnaire
                .GetPrefilledQuestions()
                .Select(questionId =>
                    this.CreateInterviewEntityViewModel(
                        entityId: questionId,
                        rosterVector: RosterVector.Empty,
                        entityModelType: GetQuestionModelType(questionId, questionnaire),
                        interviewId: interviewId,
                        navigationState: null));
        }

        private IInterviewEntityViewModel CreateInterviewEntityViewModel(
            Guid entityId,
            decimal[] rosterVector,
            Type entityModelType,
            string interviewId,
            NavigationState navigationState)
        {
            var identity = new Identity(entityId, rosterVector);

            if (!this.EntityTypeToViewModelMap.ContainsKey(entityModelType))
            {
                var text = (StaticTextViewModel)this.EntityTypeToViewModelMap[typeof(StaticTextModel)].Invoke();
                text.StaticText = entityModelType.ToString();
                return text;
            }

            var viewModelActivator = this.EntityTypeToViewModelMap[entityModelType];

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