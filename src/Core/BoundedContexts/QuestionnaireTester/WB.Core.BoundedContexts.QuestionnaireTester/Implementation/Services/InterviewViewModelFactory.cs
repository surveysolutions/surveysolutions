using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using Main.Core.Entities.SubEntities.Question;
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

        public Task<IEnumerable> GetEntitiesAsync(string interviewId, Identity groupIdentity)
        {
            return Task.Run(()=> GenerateViewModels(interviewId, groupIdentity));
        }

        public Task<IEnumerable> GetPrefilledQuestionsAsync(string interviewId)
        {
            return Task.Run(() => GetPrefilledQuestionsImpl(interviewId));
        }

        private IEnumerable GenerateViewModels(string interviewId, Identity chapterIdentity)
        {
            var interview = this.plainStorageInterviewAccessor.Get(interviewId);
            var questionnaire = this.plainQuestionnaireRepository.Get(interview.QuestionnaireId);

            if (chapterIdentity == null || chapterIdentity.Id == Guid.Empty)
            {
                chapterIdentity = new Identity(questionnaire.GroupsWithoutNestedChildren.Keys.First(), new decimal[0]);
            }

            if (!questionnaire.GroupsWithoutNestedChildren.ContainsKey(chapterIdentity.Id))
                throw new KeyNotFoundException("Group with id : {0} don't found".FormatString(chapterIdentity));

            var groupWithoutNestedChildren = questionnaire.GroupsWithoutNestedChildren[chapterIdentity.Id];

            var viewModels = groupWithoutNestedChildren
                .Children
                .Select(child => CreateInterviewItemViewModel(child.Id, chapterIdentity.RosterVector, child.ModelType, interview, questionnaire))
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
                throw new ArgumentOutOfRangeException("entityModelType");
            }

            var viewModelActivator = QuestionnaireEntityTypeToViewModelMap[entityModelType];

            BaseInterviewItemViewModel viewModel = viewModelActivator.Invoke();

            viewModel.Init(identity, interview, questionnaire);
            return viewModel;
        }
    }
}