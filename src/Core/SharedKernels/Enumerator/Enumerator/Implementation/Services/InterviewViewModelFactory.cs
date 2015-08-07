using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cirrious.CrossCore;
using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.BoundedContexts.Tester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.Tester.Properties;
using WB.Core.BoundedContexts.Tester.Repositories;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.BoundedContexts.Tester.ViewModels.InterviewDetails;
using WB.Core.BoundedContexts.Tester.ViewModels.InterviewEntities;
using WB.Core.BoundedContexts.Tester.ViewModels.Questions;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using GroupViewModel = WB.Core.BoundedContexts.Tester.ViewModels.Groups.GroupViewModel;

namespace WB.Core.BoundedContexts.Tester.Implementation.Services
{
    internal class InterviewViewModelFactory : IInterviewViewModelFactory
    {
        private readonly IPlainKeyValueStorage<QuestionnaireModel> plainQuestionnaireRepository;
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
                { typeof(FilteredSingleOptionQuestionModel), Load<FilteredSingleOptionQuestionViewModel> },
                { typeof(CascadingSingleOptionQuestionModel), Load<CascadingSingleOptionQuestionViewModel> },
                { typeof(DateTimeQuestionModel), Load<DateTimeQuestionViewModel> },
                { typeof(MultiOptionQuestionModel), Load<MultiOptionQuestionViewModel> },
                { typeof(LinkedMultiOptionQuestionModel), Load<MultiOptionLinkedQuestionViewModel> },
                { typeof(GpsCoordinatesQuestionModel), Load<GpsCoordinatesQuestionViewModel> },
                { typeof(MultimediaQuestionModel), Load<MultimedaQuestionViewModel> },
                { typeof(QRBarcodeQuestionModel), Load<QRBarcodeQuestionViewModel> },
                { typeof(GroupModel), Load<GroupViewModel> },
                { typeof(RosterModel), Load<GroupViewModel>}
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

        public IEnumerable<IInterviewEntityViewModel> GetEntities(string interviewId, Identity groupIdentity, NavigationState navigationState)
        {
            return GenerateViewModels(interviewId, groupIdentity, navigationState);
        }

        public IEnumerable<IInterviewEntityViewModel> GetPrefilledQuestions(string interviewId)
        {
            return GetPrefilledQuestionsImpl(interviewId);
        }

        private IEnumerable<IInterviewEntityViewModel> GenerateViewModels(string interviewId, Identity groupIdentity, NavigationState navigationState)
        {
            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.plainQuestionnaireRepository.GetById(interview.QuestionnaireId);

            if (groupIdentity == null || groupIdentity.Id == Guid.Empty)
            {
                groupIdentity = new Identity(questionnaire.GroupsWithFirstLevelChildrenAsReferences.Keys.First(), new decimal[0]);
            }

            if (!questionnaire.GroupsWithFirstLevelChildrenAsReferences.ContainsKey(groupIdentity.Id))
                throw new KeyNotFoundException(string.Format("Group with id : {0} don't found", groupIdentity));

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
            var questionnaire = this.plainQuestionnaireRepository.GetById(interview.QuestionnaireId);

            return questionnaire.PrefilledQuestionsIds.Select(
                question => CreateInterviewEntityViewModel(
                    entityId: question.Id,
                    rosterVector: new decimal[0],
                    entityModelType: question.ModelType,
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

        public IEnumerable<dynamic> GetCompleteScreenEntities(string interviewId)
        {
            var result = new List<IInterviewEntityViewModel>();

            var text = (StaticTextViewModel)EntityTypeToViewModelMap[typeof(StaticTextModel)].Invoke();
            text.StaticText = UIResources.Interview_Complete_Screen_Description;
            result.Add(text);

            var statistics = Load<InterviewCompletionStatisticsViewModel>();
            statistics.Init(interviewId, null, null);
            result.Add(statistics);

            var completionInterview = Load<InterviewStatusChangeViewModel>();
            completionInterview.Init(interviewId, null, null);
            result.Add(completionInterview);

            return result;
        }

        public T GetNew<T>() where T : class
        {
            return Load<T>();
        }
    }
}