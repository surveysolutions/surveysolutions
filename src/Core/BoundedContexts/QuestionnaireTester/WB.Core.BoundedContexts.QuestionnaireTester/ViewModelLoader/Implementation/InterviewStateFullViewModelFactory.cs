using System;
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

        public InterviewStateFullViewModelFactory(
            IPlainRepository<QuestionnaireModel> plainQuestionnaireRepository,
            IPlainRepository<InterviewModel> plainStorageInterviewAccessor)
        {
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.plainStorageInterviewAccessor = plainStorageInterviewAccessor;
        }

        public Task<ObservableCollection<MvxViewModel>> LoadAsync(string interviewId, string chapterId)
        {
            return Task.Run(()=> GenerateViewModels(interviewId, chapterId));
        }

        private ObservableCollection<MvxViewModel> GenerateViewModels(string interviewId, string chapterId)
        {
            var interview = this.plainStorageInterviewAccessor.Get(interviewId);
            var questionnaire = this.plainQuestionnaireRepository.Get(interview.QuestionnaireId.FormatGuid());

            var chapterIdGuid = string.IsNullOrEmpty(chapterId) 
                ? questionnaire.GroupsWithoutNestedChildren.Keys.First()
                : Guid.Parse(chapterId);

            
            if (chapterId != null && questionnaire.GroupsWithoutNestedChildren.ContainsKey(chapterIdGuid))
                    throw new KeyNotFoundException("Group with id : {0} don't found".FormatString(chapterId));

            var layout = questionnaire.GroupsWithoutNestedChildren[chapterIdGuid];

            ObservableCollection<MvxViewModel> entities = new ObservableCollection<MvxViewModel>();

            var rosterVector = new decimal[0];

            foreach (var itemPlaceholder in layout.Placeholders)
            {
                if (itemPlaceholder is RosterPlaceholderModel)
                {
                    var rosterModel = itemPlaceholder as RosterPlaceholderModel;

                    var identity = new Identity(rosterModel.Id, rosterVector);
                    var questionViewModel = Mvx.Create<RosterReferenceViewModel>();
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
            var identity = new Identity(groupId, rosterVector);
            
            var questionViewModel = Mvx.Create<GroupReferenceViewModel>();
            questionViewModel.Init(identity, interview, questionnaire);
            return questionViewModel;
        }

        private static BaseInterviewItemViewModel CreateQuestionViewModel(
            Guid questionId,
            decimal[] rosterVector,
            InterviewModel interview,
            QuestionnaireModel questionnaire)
        {
            var identity = new Identity(questionId, rosterVector);

            var questionModel = questionnaire.Questions[questionId];

            BaseInterviewItemViewModel questionViewModel;

            switch (questionModel.Type)
            {
                case QuestionModelType.SingleOption:
                    questionViewModel = Mvx.Create<SingleOptionQuestionViewModel>();
                    break;
                case QuestionModelType.LinkedSingleOption:
                    questionViewModel = Mvx.Create<LinkedSingleOptionQuestionViewModel>();
                    break;
                case QuestionModelType.MultiOption:
                    questionViewModel = Mvx.Create<MultiOptionQuestionViewModel>();
                    break;
                case QuestionModelType.LinkedMultiOption:
                    questionViewModel = Mvx.Create<LinkedMultiOptionQuestionViewModel>();
                    break;
                case QuestionModelType.IntegerNumeric:
                    questionViewModel = Mvx.Create<IntegerNumericQuestionViewModel>();
                    break;
                case QuestionModelType.RealNumeric:
                    questionViewModel = Mvx.Create<RealNumericQuestionViewModel>();
                    break;
                case QuestionModelType.MaskedText:
                    questionViewModel = Mvx.Create<MaskedTextQuestionViewModel>();
                    break;
                case QuestionModelType.TextList:
                    questionViewModel = Mvx.Create<TextListQuestionViewModel>();
                    break;
                case QuestionModelType.QrBarcode:
                    questionViewModel = Mvx.Create<QrBarcodeQuestionViewModel>();
                    break;
                case QuestionModelType.Multimedia:
                    questionViewModel = Mvx.Create<MultimediaQuestionViewModel>();
                    break;
                case QuestionModelType.DateTime:
                    questionViewModel = Mvx.Create<DateTimeQuestionViewModel>();
                    break;
                case QuestionModelType.GpsCoordinates:
                    questionViewModel = Mvx.Create<GpsCoordinatesQuestionViewModel>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
           
            questionViewModel.Init(identity, interview, questionnaire);
            return questionViewModel;
        }

        public Task<ObservableCollection<MvxViewModel>> GetPrefilledQuestionsAsync(string interviewId)
        {
            return new Task<ObservableCollection<MvxViewModel>>(() =>
                    {
                        return new ObservableCollection<MvxViewModel>(new[] { new MaskedTextQuestionViewModel(null, null), });
                    });
        }
    }
}