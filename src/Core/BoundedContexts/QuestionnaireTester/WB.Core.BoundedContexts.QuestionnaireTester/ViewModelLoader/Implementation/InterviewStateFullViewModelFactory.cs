using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;


namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModelLoader.Implementation
{
    internal class InterviewStateFullViewModelFactory : IInterviewStateFullViewModelFactory
    {
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;
        private readonly IPlainInterviewRepository plainStorageInterviewAccessor;

        public InterviewStateFullViewModelFactory(
            IPlainQuestionnaireRepository plainQuestionnaireRepository,
            IPlainInterviewRepository plainStorageInterviewAccessor)
        {
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.plainStorageInterviewAccessor = plainStorageInterviewAccessor;
        }

        private readonly Dictionary<Type, Func<Identity, InterviewModel, QuestionnaireDocument, MvxViewModel>> mapQuestions = new Dictionary<Type, Func<Identity, InterviewModel, QuestionnaireDocument, MvxViewModel>>()
        {
            { typeof(Group), (qIdentity, interview, questionnaire) => CreateViewModel<GroupReferenceViewModel>(vm => vm.Init(qIdentity, interview, questionnaire)) },
            { typeof(StaticText), (qIdentity, interview, questionnaire) => CreateViewModel<StaticTextViewModel>(vm => vm.Init(qIdentity, questionnaire)) },
            { typeof(TextQuestion), (qIdentity, interview, questionnaire) => CreateViewModel<TextQuestionViewModel>(vm => vm.Init(qIdentity, interview, questionnaire)) },
        };

        public Task<List<MvxViewModel>> LoadAsync(string interviewId, string chapterId)
        {
            return Task.Run(()=> GenerateViewModels(interviewId, chapterId));
        }

        private List<MvxViewModel> GenerateViewModels(string interviewId, string chapterId)
        {
            var interview = this.plainStorageInterviewAccessor.GetInterview(interviewId);
            var questionnaire = this.plainQuestionnaireRepository.GetQuestionnaireDocument(interview.QuestionnaireId,
                interview.QuestionnaireVersion);

            IComposite loyout;

            if (chapterId.IsNullOrEmpty())
            {
                loyout = questionnaire.Children.First();
            }
            else
            {
                Guid chapterIdGuid = new Guid(chapterId);
                loyout = questionnaire.Find<IGroup>(chapterIdGuid);
                if (chapterId != null && loyout == null)
                    throw new KeyNotFoundException("Group with id : {0} don't found".FormatString(chapterId));
            }

            List<MvxViewModel> entities = new List<MvxViewModel>();

            foreach (var child in loyout.Children)
            {
                var entityType = child.GetType();

                if (!this.mapQuestions.ContainsKey(entityType))
                {
                    entities.Add(new StaticTextViewModel() {Title = child.ToString()});
                    continue; // temporaly ignore unknown types
                }

                var mapQuestionFunc = this.mapQuestions[entityType];
                Identity identity = new Identity(child.PublicKey, new decimal[0]); // TODO SPuV: rosterVecror ????
                var entityViewModel = mapQuestionFunc.Invoke(identity, interview, questionnaire);

                entities.Add(entityViewModel);
            }
            return entities;
        }

        private static T CreateViewModel<T>(Action<T> intializer) where T : class
        {
            T viewModel = Mvx.Create<T>();
            intializer.Invoke(viewModel);
            return viewModel;
        }

        public Task<ObservableCollection<MvxViewModel>> GetPrefilledQuestionsAsync(string interviewId)
        {
            return
                new Task<ObservableCollection<MvxViewModel>>(() => new ObservableCollection<MvxViewModel>(new []{ new TextQuestionViewModel(null, null), }));
        }
    }
}