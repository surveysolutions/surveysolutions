using System;
using System.Collections.Generic;
using System.Linq;
using Cirrious.CrossCore;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
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
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireModelRepository;
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
                { typeof(FilteredSingleOptionQuestionModel), Load<FilteredSingleOptionQuestionViewModel> },
                { typeof(CascadingSingleOptionQuestionModel), Load<CascadingSingleOptionQuestionViewModel> },
                { typeof(DateTimeQuestionModel), Load<DateTimeQuestionViewModel> },
                { typeof(MultiOptionQuestionModel), Load<MultiOptionQuestionViewModel> },
                { typeof(LinkedMultiOptionQuestionModel), Load<MultiOptionLinkedQuestionViewModel> },
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
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireModelRepository,
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
            var questionnaire = this.questionnaireModelRepository.GetById(interview.QuestionnaireId);

            if (!questionnaire.GroupsWithFirstLevelChildrenAsReferences.ContainsKey(groupIdentity.Id))
                throw new KeyNotFoundException($"Group with id {groupIdentity.Id.FormatGuid()} don't found");

            var referencesOfQuestionnaireEntities = questionnaire.GroupsWithFirstLevelChildrenAsReferences[groupIdentity.Id].Children;

            var groupWithoutNestedChildren = interview.GetInterviewerEntities(groupIdentity);

            return groupWithoutNestedChildren.Select(questionnaireEntity => this.CreateInterviewEntityViewModel(
                entityId: questionnaireEntity.Id,
                rosterVector: questionnaireEntity.RosterVector,
                entityModelType: referencesOfQuestionnaireEntities.Find(y => y.Id == questionnaireEntity.Id).ModelType,
                interviewId: interviewId,
                navigationState: navigationState));
        }

        private IEnumerable<IInterviewEntityViewModel> GetPrefilledQuestionsImpl(string interviewId)
        {
            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity);
            var questionnaireModel = this.questionnaireModelRepository.GetById(interview.QuestionnaireId);

            return questionnaire
                .GetPrefilledQuestions()
                .Select(questionId =>
                    this.CreateInterviewEntityViewModel(
                        entityId: questionId,
                        rosterVector: RosterVector.Empty,
                        entityModelType: questionnaireModel.Questions[questionId].GetType(),
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