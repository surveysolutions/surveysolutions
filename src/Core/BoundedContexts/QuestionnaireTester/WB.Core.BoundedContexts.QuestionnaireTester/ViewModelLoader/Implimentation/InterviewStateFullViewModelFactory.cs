using System;
using System.Collections.Generic;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.QuestionnaireTester.Model;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using StaticTextViewModel = WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels.StaticTextViewModel;


namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModelLoader.Implimentation
{
    internal class InterviewStateFullViewModelFactory : IInterviewStateFullViewModelFactory
    {
        private readonly IPlainStorageAccessor<QuestionnaireDocument> plainStorageQuestionnaireAccessor;
        private readonly IPlainStorageAccessor<InterviewModel> plainStorageInterviewAccessor;

        public InterviewStateFullViewModelFactory(
            IPlainStorageAccessor<QuestionnaireDocument> plainStorageQuestionnaireAccessor,
            IPlainStorageAccessor<InterviewModel> plainStorageInterviewAccessor)
        {
            this.plainStorageQuestionnaireAccessor = plainStorageQuestionnaireAccessor;
            this.plainStorageInterviewAccessor = plainStorageInterviewAccessor;
        }

        private readonly Dictionary<Type, Func<Identity, InterviewModel, QuestionnaireDocument, MvxViewModel>> mapQuestions = new Dictionary<Type, Func<Identity, InterviewModel, QuestionnaireDocument, MvxViewModel>>()
        {
            { typeof(Group), (qIdentity, interview, questionnaire) => CreateViewModel<GroupReferanceViewModel>(vm => vm.Init(qIdentity, interview, questionnaire)) },
            { typeof(StaticText), (qIdentity, interview, questionnaire) => CreateViewModel<StaticTextViewModel>(vm => vm.Init(qIdentity, questionnaire)) },
            { typeof(TextQuestion), (qIdentity, interview, questionnaire) => CreateViewModel<TextQuestionViewModel>(vm => vm.Init(qIdentity, interview, questionnaire)) },
        };

        public IEnumerable<MvxViewModel> Load(string interviewId, string chapterId)
        {
            var interview = plainStorageInterviewAccessor.GetById(interviewId);
            var questionaryId = interview.QuestionaryId.FormatGuid();

            var questionnaire = plainStorageQuestionnaireAccessor.GetById(questionaryId);
            
            Guid chapterIdGuid = new Guid(chapterId);
            var @group = questionnaire.Find<IGroup>(chapterIdGuid);
            if (chapterId != null && @group == null)
                throw new KeyNotFoundException("Grup with id : {0} don't found".FormatString(chapterId));

            List<MvxViewModel> entities = new List<MvxViewModel>();

            foreach (var child in @group.Children)
            {
                var entityType = child.GetType();

                if (!mapQuestions.ContainsKey(entityType))
                    continue; // temporaly ignore unknown types

                var mapQuestionFunc = mapQuestions[entityType];
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
    }
}