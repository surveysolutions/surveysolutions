using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModelLoader.Implementation
{
    internal class InterviewStateFullViewModelFactory : IInterviewStateFullViewModelFactory
    {
        private readonly IPlainRepository<QuestionnaireModel> plainQuestionnaireRepository;
        private readonly IPlainRepository<InterviewModel> plainStorageInterviewAccessor;

        private static readonly Dictionary<QuestionModelType, Func<BaseInterviewItemViewModel>> questionTypeToViewModelMap = 
            new Dictionary<QuestionModelType, Func<BaseInterviewItemViewModel>>
            {
                { QuestionModelType.SingleOption, Mvx.Create<SingleOptionQuestionViewModel> },
                { QuestionModelType.LinkedSingleOption, Mvx.Create<LinkedSingleOptionQuestionViewModel> },
                { QuestionModelType.MultiOption, Mvx.Create<MultiOptionQuestionViewModel> },
                { QuestionModelType.LinkedMultiOption, Mvx.Create<LinkedMultiOptionQuestionViewModel> },
                { QuestionModelType.IntegerNumeric, Mvx.Create<IntegerNumericQuestionViewModel> },
                { QuestionModelType.RealNumeric, Mvx.Create<RealNumericQuestionViewModel> },
                { QuestionModelType.MaskedText, Mvx.Create<MaskedTextQuestionViewModel> },
                { QuestionModelType.TextList, Mvx.Create<TextListQuestionViewModel> },
                { QuestionModelType.QrBarcode, Mvx.Create<QrBarcodeQuestionViewModel> },
                { QuestionModelType.Multimedia, Mvx.Create<MultimediaQuestionViewModel> },
                { QuestionModelType.DateTime, Mvx.Create<DateTimeQuestionViewModel> },
                { QuestionModelType.GpsCoordinates, Mvx.Create<GpsCoordinatesQuestionViewModel> }
            };

        public InterviewStateFullViewModelFactory(
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

            var entities = new List<MvxViewModel>();

            var rosterVector = new decimal[0];

            foreach (var itemPlaceholder in groupWithoutNestedChildren.Placeholders)
            {
                if (itemPlaceholder is RosterPlaceholderModel)
                {
                    var rosterModel = itemPlaceholder as RosterPlaceholderModel;

                    var identity = new Identity(rosterModel.Id, rosterVector);
                    var questionViewModel = Mvx.Create<RostersReferenceViewModel>();
                    questionViewModel.Init(identity, interview, questionnaire);
                    entities.Add(questionViewModel);
                }

                if (itemPlaceholder is GroupPlaceholderModel)
                {
                    var groupModel = itemPlaceholder as GroupPlaceholderModel;
                    var questionViewModel = CreateGroupReferenceViewModel(groupModel.Id, rosterVector, interview, questionnaire);
                    entities.Add(questionViewModel);
                }

                if (itemPlaceholder is QuestionPlaceholderModel)
                {
                    var questionPlaceholder = itemPlaceholder as QuestionPlaceholderModel;
                    var questionViewModel = CreateQuestionViewModel(questionPlaceholder.Id, rosterVector, interview, questionnaire);
                    entities.Add(questionViewModel);
                }

                if (itemPlaceholder is StaticTextModel)
                {
                    entities.Add(new StaticTextViewModel { Title = itemPlaceholder.Title });
                }
            }
            return entities;
        }

        private static GroupReferenceViewModel CreateGroupReferenceViewModel(
            Guid groupId,
            decimal[] rosterVector,
            InterviewModel interview,
            QuestionnaireModel questionnaire)
        {
            var questionViewModel = Mvx.Create<GroupReferenceViewModel>();
            questionViewModel.Init(new Identity(groupId, rosterVector), interview, questionnaire);
            return questionViewModel;
        }

        private static BaseInterviewItemViewModel CreateQuestionViewModel(
            Guid questionId,
            decimal[] rosterVector,
            InterviewModel interview,
            QuestionnaireModel questionnaire)
        {
            var questionModel = questionnaire.Questions[questionId];

            if (!questionTypeToViewModelMap.ContainsKey(questionModel.Type))
                throw new ArgumentOutOfRangeException();

            var questionViewModelCreator = questionTypeToViewModelMap[questionModel.Type];

            BaseInterviewItemViewModel questionViewModel = questionViewModelCreator.Invoke();
            
            questionViewModel.Init(new Identity(questionId, rosterVector), interview, questionnaire);

            return questionViewModel;
        }

        public Task<IEnumerable> GetPrefilledQuestionsAsync(string interviewId)
        {
            return Task.Run(() => GetPrefilledQuestionsImpl(interviewId));
        }

        private IEnumerable GetPrefilledQuestionsImpl(string interviewId)
        {
            var interview = this.plainStorageInterviewAccessor.Get(interviewId);
            var questionnaire = this.plainQuestionnaireRepository.Get(interview.QuestionnaireId);

            return questionnaire.PrefilledQuestionsIds.Select(x => CreateQuestionViewModel(x, new decimal[0], interview, questionnaire));
        }
    }
}