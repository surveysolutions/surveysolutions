using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cirrious.CrossCore;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities.QuestionModels;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services
{
    public class InterviewViewModelFactory : IInterviewViewModelFactory
    {
        private readonly IPlainKeyValueStorage<QuestionnaireModel> plainQuestionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;

        private static readonly Dictionary<Type, Func<IInterviewEntityViewModel>> QuestionnaireEntityTypeToViewModelMap = 
            new Dictionary<Type, Func<IInterviewEntityViewModel>>
            {
                { typeof(StaticTextModel), Load<StaticTextViewModel> },
                { typeof(MaskedTextQuestionModel), Load<TextQuestionViewModel> },
                { typeof(SingleOptionQuestionModel), Load<SingleOptionQuestionViewModel> },
                { typeof(MultiOptionQuestionModel), Load<MultiOptionQuestionViewModel> },
                { typeof(GpsCoordinatesQuestionModel), Load<GpsCoordinatesQuestionViewModel> }
            };

        private static T Load<T>() where T : class
        {
            return Mvx.Resolve<T>();
        }

        public InterviewViewModelFactory(
            IPlainKeyValueStorage<QuestionnaireModel> plainQuestionnaireRepository,
            IStatefulInterviewRepository interviewRepository)
        {
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.interviewRepository = interviewRepository;
        }

        public IList GetEntities(string interviewId, Identity groupIdentity)
        {
            return GenerateViewModels(interviewId, groupIdentity);
        }

        public IList GetPrefilledQuestions(string interviewId)
        {
            return GetPrefilledQuestionsImpl(interviewId);
        }

        private IList GenerateViewModels(string interviewId, Identity groupIdentity)
        {
            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.plainQuestionnaireRepository.GetById(interview.QuestionnaireId);

            if (groupIdentity == null || groupIdentity.Id == Guid.Empty)
            {
                groupIdentity = new Identity(questionnaire.GroupsWithoutNestedChildren.Keys.First(), new decimal[0]);
            }

            if (!questionnaire.GroupsWithoutNestedChildren.ContainsKey(groupIdentity.Id))
                throw new KeyNotFoundException(string.Format("Group with id : {0} don't found", groupIdentity));

            var groupWithoutNestedChildren = questionnaire.GroupsWithoutNestedChildren[groupIdentity.Id];

            var viewModels = groupWithoutNestedChildren
                .Children
                .Select(child => CreateInterviewItemViewModel(entityId: child.Id, rosterVector: groupIdentity.RosterVector, entityModelType: child.ModelType, interviewId: interviewId))
                .ToList();

            return viewModels;
        }

        private IList GetPrefilledQuestionsImpl(string interviewId)
        {
            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.plainQuestionnaireRepository.GetById(interview.QuestionnaireId);

            return questionnaire.PrefilledQuestionsIds.Select(
                    question => CreateInterviewItemViewModel(entityId: question.Id, rosterVector: new decimal[0],
                                                             entityModelType: question.ModelType, interviewId: interviewId)).ToList();
        }

        private static IInterviewEntityViewModel CreateInterviewItemViewModel(
            Guid entityId,
            decimal[] rosterVector,
            Type entityModelType,
            string interviewId)
        {
            var identity = new Identity(entityId, rosterVector);

            if (!QuestionnaireEntityTypeToViewModelMap.ContainsKey(entityModelType))
            {
                //throw new ArgumentOutOfRangeException("entityModelType", entityModelType, "View model is not registered");
                var text = (StaticTextViewModel)QuestionnaireEntityTypeToViewModelMap[typeof(StaticTextModel)].Invoke();
                text.StaticText = entityModelType.ToString();
                return text;
            }

            var viewModelActivator = QuestionnaireEntityTypeToViewModelMap[entityModelType];

            IInterviewEntityViewModel viewModel = viewModelActivator.Invoke();

            viewModel.Init(interviewId: interviewId, entityIdentity: identity);
            return viewModel;
        }
    }
}