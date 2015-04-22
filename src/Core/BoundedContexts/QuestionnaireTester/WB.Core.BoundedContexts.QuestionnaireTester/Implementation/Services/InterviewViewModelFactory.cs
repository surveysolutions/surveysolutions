using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
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

        private static readonly Dictionary<Type, Func<BaseInterviewItemViewModel>> QuestionnaireEntityTypeToViewModelMap = 
            new Dictionary<Type, Func<BaseInterviewItemViewModel>>
            {
                { typeof(StaticTextModel), Mvx.Create<StaticTextViewModel> },
                { typeof(GroupModel), Mvx.Create<GroupReferenceViewModel> },
                { typeof(RosterModel), Mvx.Create<RostersReferenceViewModel> },
                // questions
                { typeof(SingleOptionQuestionModel), Mvx.Create<SingleOptionQuestionViewModel> },
                { typeof(LinkedSingleOptionQuestionModel), Mvx.Create<LinkedSingleOptionQuestionViewModel> },
                { typeof(MultiOptionQuestionModel), Mvx.Create<MultiOptionQuestionViewModel> },
                { typeof(LinkedMultiOptionQuestionModel), Mvx.Create<LinkedMultiOptionQuestionViewModel> },
                { typeof(IntegerNumericQuestionModel), Mvx.Create<IntegerNumericQuestionViewModel> },
                { typeof(RealNumericQuestionModel), Mvx.Create<RealNumericQuestionViewModel> },
                { typeof(MaskedTextQuestionModel), Mvx.Create<MaskedTextQuestionViewModel> },
                { typeof(TextListQuestionModel), Mvx.Create<TextListQuestionViewModel> },
                { typeof(QrBarcodeQuestionModel), Mvx.Create<QrBarcodeQuestionViewModel> },
                { typeof(MultimediaQuestionModel), Mvx.Create<MultimediaQuestionViewModel> },
                { typeof(DateTimeQuestionModel), Mvx.Create<DateTimeQuestionViewModel> },
                { typeof(GpsCoordinatesQuestionModel), Mvx.Create<GpsCoordinatesQuestionViewModel> }
            };

        public InterviewViewModelFactory(
            IPlainRepository<QuestionnaireModel> plainQuestionnaireRepository,
            IPlainRepository<InterviewModel> plainStorageInterviewAccessor)
        {
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.plainStorageInterviewAccessor = plainStorageInterviewAccessor;
        }

        public Task<IEnumerable> LoadAsync(string interviewId, string chapterId)
        {
            return Task.Run(()=> GenerateViewModels(interviewId, chapterId));
        }

        public Task<IEnumerable> GetPrefilledQuestionsAsync(string interviewId)
        {
            return Task.Run(() => GetPrefilledQuestionsImpl(interviewId));
        }

        private IEnumerable GenerateViewModels(string interviewId, string chapterId)
        {
            var interview = this.plainStorageInterviewAccessor.Get(interviewId);
            var questionnaire = this.plainQuestionnaireRepository.Get(interview.QuestionnaireId);

            var chapterIdGuid = string.IsNullOrEmpty(chapterId) 
                ? questionnaire.GroupsWithoutNestedChildren.Keys.First()
                : Guid.Parse(chapterId);

            if (chapterId != null && questionnaire.GroupsWithoutNestedChildren.ContainsKey(chapterIdGuid))
                    throw new KeyNotFoundException("Group with id : {0} don't found".FormatString(chapterId));

            var groupWithoutNestedChildren = questionnaire.GroupsWithoutNestedChildren[chapterIdGuid];

            var rosterVector = new decimal[0];

            var viewModels = groupWithoutNestedChildren
                .Children
                .Select(child => CreateInterviewItemViewModel(child.Id, rosterVector, child.ModelType, interview, questionnaire))
                .ToList();

            return viewModels;
        }

        private IEnumerable GetPrefilledQuestionsImpl(string interviewId)
        {
            var interview = this.plainStorageInterviewAccessor.Get(interviewId);
            var questionnaire = this.plainQuestionnaireRepository.Get(interview.QuestionnaireId);

            return questionnaire.PrefilledQuestionsIds.Select(x => CreateInterviewItemViewModel(x.Id, new decimal[0], x.ModelType, interview, questionnaire));
        }

        private static BaseInterviewItemViewModel CreateInterviewItemViewModel(
            Guid entityId,
            decimal[] rosterVector,
            Type entityModelType,
            InterviewModel interview,
            QuestionnaireModel questionnaire)
        {
            var identity = new Identity(entityId, rosterVector);

            if (!QuestionnaireEntityTypeToViewModelMap.ContainsKey(entityModelType))
            {
                throw new ArgumentOutOfRangeException();
            }

            var viewModelActivator = QuestionnaireEntityTypeToViewModelMap[entityModelType];

            BaseInterviewItemViewModel viewModel = viewModelActivator.Invoke();

            viewModel.Init(identity, interview, questionnaire);
            return viewModel;
        }
    }
}