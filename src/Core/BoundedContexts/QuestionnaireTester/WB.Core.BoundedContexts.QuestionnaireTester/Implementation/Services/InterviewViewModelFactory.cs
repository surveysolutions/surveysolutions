using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities.QuestionModels;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services
{
    internal class InterviewViewModelFactory : IInterviewViewModelFactory
    {
        private readonly IPlainRepository<QuestionnaireModel> plainQuestionnaireRepository;
        private readonly IPlainRepository<InterviewModel> plainStorageInterviewAccessor;

        private static readonly Dictionary<Type, Func<IInterviewEntity>> QuestionnaireEntityTypeToViewModelMap = 
            new Dictionary<Type, Func<IInterviewEntity>>
            {
                { typeof(StaticTextModel),                 Load<StaticTextViewModel>                 },
                // questions
                { typeof(MaskedTextQuestionModel),         Load<MaskedTextQuestionViewModel>         },
                { typeof(GpsCoordinatesQuestionModel),     Load<GpsCoordinatesQuestionViewModel>     },
                { typeof(SingleOptionQuestionModel), Mvx.Create<QuestionContainerViewModel<SingleOptionQuestionViewModel>> },
                { typeof(LinkedSingleOptionQuestionModel), Mvx.Create<QuestionContainerViewModel<LinkedSingleOptionQuestionViewModel>> },
                { typeof(MultiOptionQuestionModel), Mvx.Create<QuestionContainerViewModel<MultiOptionQuestionViewModel>> },
                { typeof(LinkedMultiOptionQuestionModel), Mvx.Create<QuestionContainerViewModel<LinkedMultiOptionQuestionViewModel>> },
                { typeof(IntegerNumericQuestionModel), Mvx.Create<QuestionContainerViewModel<IntegerNumericQuestionViewModel>> },
                { typeof(RealNumericQuestionModel), Mvx.Create<QuestionContainerViewModel<RealNumericQuestionViewModel>> },
                { typeof(MaskedTextQuestionModel), Mvx.Create<QuestionContainerViewModel<MaskedTextQuestionViewModel>> },
                { typeof(TextListQuestionModel), Mvx.Create<QuestionContainerViewModel<TextListQuestionViewModel>> },
                { typeof(QrBarcodeQuestionModel), Mvx.Create<QuestionContainerViewModel<QrBarcodeQuestionViewModel>> },
                { typeof(MultimediaQuestionModel), Mvx.Create<QuestionContainerViewModel<MultimediaQuestionViewModel>> },
                { typeof(DateTimeQuestionModel), Mvx.Create<QuestionContainerViewModel<DateTimeQuestionViewModel>> },
                { typeof(GpsCoordinatesQuestionModel), Mvx.Create<QuestionContainerViewModel<GpsCoordinatesQuestionViewModel>> }
            };

        private static T Load<T>() where T : class
        {
            return Mvx.Resolve<T>();
        }

        public InterviewViewModelFactory(
            IPlainRepository<QuestionnaireModel> plainQuestionnaireRepository,
            IPlainRepository<InterviewModel> plainStorageInterviewAccessor)
        {
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.plainStorageInterviewAccessor = plainStorageInterviewAccessor;
        }

        public Task<IEnumerable> GetEntitiesAsync(string interviewId, Identity groupIdentity)
        {
            return Task.Run(()=> GenerateViewModels(interviewId, groupIdentity));
        }

        public Task<IEnumerable> GetPrefilledQuestionsAsync(string interviewId)
        {
            return Task.Run(() => GetPrefilledQuestionsImpl(interviewId));
        }

        private IEnumerable GenerateViewModels(string interviewId, Identity groupIdentity)
        {
            var interview = this.plainStorageInterviewAccessor.Get(interviewId);
            var questionnaire = this.plainQuestionnaireRepository.Get(interview.QuestionnaireId);

            if (groupIdentity == null || groupIdentity.Id == Guid.Empty)
            {
                groupIdentity = new Identity(questionnaire.GroupsWithoutNestedChildren.Keys.First(), new decimal[0]);
            }

            if (!questionnaire.GroupsWithoutNestedChildren.ContainsKey(groupIdentity.Id))
                throw new KeyNotFoundException("Group with id : {0} don't found".FormatString(groupIdentity));

            var groupWithoutNestedChildren = questionnaire.GroupsWithoutNestedChildren[groupIdentity.Id];

            var viewModels = groupWithoutNestedChildren
                .Children
                .Select(child => CreateInterviewItemViewModel(entityId: child.Id, rosterVector: groupIdentity.RosterVector, entityModelType: child.ModelType, interviewId: interviewId))
                .ToList();

            return viewModels;
        }

        private IEnumerable GetPrefilledQuestionsImpl(string interviewId)
        {
            var interview = this.plainStorageInterviewAccessor.Get(interviewId);
            var questionnaire = this.plainQuestionnaireRepository.Get(interview.QuestionnaireId);

            return questionnaire.PrefilledQuestionsIds.Select(question => CreateInterviewItemViewModel(entityId: question.Id, rosterVector: new decimal[0], entityModelType: question.ModelType, interviewId: interviewId));
        }

        private static IInterviewEntity CreateInterviewItemViewModel(
            Guid entityId,
            decimal[] rosterVector,
            Type entityModelType,
            string interviewId)
        {
            var identity = new Identity(entityId, rosterVector);

            if (!QuestionnaireEntityTypeToViewModelMap.ContainsKey(entityModelType))
            {
                throw new ArgumentOutOfRangeException("entityModelType");
            }

            var viewModelActivator = QuestionnaireEntityTypeToViewModelMap[entityModelType];

            IInterviewEntity viewModel = viewModelActivator.Invoke();

            viewModel.Init(interviewId: interviewId, identity: identity);
            return viewModel;
        }
    }
}