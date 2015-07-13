﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cirrious.CrossCore;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.Groups;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.InterviewEntities;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.Questions;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using GroupViewModel = WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.Groups.GroupViewModel;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services
{
    internal class InterviewViewModelFactory : IInterviewViewModelFactory
    {
        private readonly IPlainKeyValueStorage<QuestionnaireModel> plainQuestionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;

        private Dictionary<Type, Func<IInterviewEntityViewModel>> QuestionnaireEntityTypeToViewModelMap =
            new Dictionary<Type, Func<IInterviewEntityViewModel>>
            {
                { typeof(StaticTextModel), Load<StaticTextViewModel> },
                { typeof(IntegerNumericQuestionModel), Load<IntegerQuestionViewModel> },
                { typeof(RealNumericQuestionModel), Load<RealQuestionViewModel> },
                { typeof(TextQuestionModel), Load<TextQuestionViewModel> },
                { typeof(TextListQuestionModel), Load<TextListQuestionViewModel> },
                { typeof(SingleOptionQuestionModel), Load<SingleOptionQuestionViewModel> },
                { typeof(LinkedSingleOptionQuestionModel), Load<SingleOptionLinkedQuestionViewModel> },
                { typeof(FilteredSingleOptionQuestionModel), Load<FilteredSingleOptionQuestionViewModel> },
                { typeof(CascadingSingleOptionQuestionModel), Load<CascadingSingleOptionQuestionViewModel> },
                { typeof(DateTimeQuestionModel), Load<DateTimeQuestionViewModel> },
                { typeof(MultiOptionQuestionModel), Load<MultiOptionQuestionViewModel> },
                { typeof(LinkedMultiOptionQuestionModel), Load<MultiOptionLinkedQuestionViewModel> },
                { typeof(GpsCoordinatesQuestionModel), Load<GpsCoordinatesQuestionViewModel> },
                { typeof(MultimediaQuestionModel), Load<MultimedaQuestionViewModel> },
                { typeof(QRBarcodeQuestionModel), Load<QRBarcodeQuestionViewModel> },
                { typeof(GroupModel), Load<GroupViewModel> },
                { typeof(RosterModel), Load<RosterViewModel> }
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

        public IList GetEntities(string interviewId, Identity groupIdentity, NavigationState navigationState)
        {
            return GenerateViewModels(interviewId, groupIdentity, navigationState);
        }

        public IList GetPrefilledQuestions(string interviewId)
        {
            return GetPrefilledQuestionsImpl(interviewId);
        }

        private IList GenerateViewModels(string interviewId, Identity groupIdentity, NavigationState navigationState)
        {
            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.plainQuestionnaireRepository.GetById(interview.QuestionnaireId);

            if (groupIdentity == null || groupIdentity.Id == Guid.Empty)
            {
                groupIdentity = new Identity(questionnaire.GroupsWithFirstLevelChildrenAsReferences.Keys.First(), new decimal[0]);
            }

            if (!questionnaire.GroupsWithFirstLevelChildrenAsReferences.ContainsKey(groupIdentity.Id))
                throw new KeyNotFoundException(string.Format("Group with id : {0} don't found", groupIdentity));

            var groupWithoutNestedChildren = questionnaire.GroupsWithFirstLevelChildrenAsReferences[groupIdentity.Id];

            var viewModels = groupWithoutNestedChildren
                .Children
                .Select(child => CreateInterviewEntityViewModel(
                    entityId: child.Id, 
                    rosterVector: groupIdentity.RosterVector, 
                    entityModelType: child.ModelType, 
                    interviewId: interviewId,
                    navigationState: navigationState))
                .ToList();

            

            return viewModels;
        }

        private IList GetPrefilledQuestionsImpl(string interviewId)
        {
            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.plainQuestionnaireRepository.GetById(interview.QuestionnaireId);

            var result = questionnaire.PrefilledQuestionsIds.Select(
                question => CreateInterviewEntityViewModel(
                    entityId: question.Id,
                    rosterVector: new decimal[0],
                    entityModelType: question.ModelType,
                    interviewId: interviewId,
                    navigationState: null)).ToList();

            var startButton = Load<StartInterviewViewModel>();
            startButton.Init(interviewId, null, null);
            result.Add(startButton);
            return result;
        }

        private IInterviewEntityViewModel CreateInterviewEntityViewModel(
            Guid entityId,
            decimal[] rosterVector,
            Type entityModelType,
            string interviewId,
            NavigationState navigationState)
        {
            var identity = new Identity(entityId, rosterVector);

            if (!QuestionnaireEntityTypeToViewModelMap.ContainsKey(entityModelType))
            {
                var text = (StaticTextViewModel)QuestionnaireEntityTypeToViewModelMap[typeof(StaticTextModel)].Invoke();
                text.StaticText = entityModelType.ToString();
                return text;
            }

            var viewModelActivator = QuestionnaireEntityTypeToViewModelMap[entityModelType];

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