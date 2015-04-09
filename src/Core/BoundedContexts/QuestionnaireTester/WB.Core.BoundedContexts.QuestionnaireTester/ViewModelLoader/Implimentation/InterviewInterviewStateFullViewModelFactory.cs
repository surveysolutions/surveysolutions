using System;
using System.Collections.Generic;
using Cirrious.MvvmCross.ViewModels;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.QuestionnaireTester.Model;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewInfrastructure;
using StaticTextViewModel = WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels.StaticTextViewModel;


namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModelLoader.Implimentation
{
    internal class InterviewInterviewStateFullViewModelFactory : IInterviewStateFullViewModelFactory
    {
        private readonly IPlainStorageAccessor<QuestionnaireDocument> plainStorageQuestionnaireAccessor;
        private readonly IPlainStorageAccessor<InterviewModel> plainStorageInterviewAccessor;

        public InterviewInterviewStateFullViewModelFactory(
            IPlainStorageAccessor<QuestionnaireDocument> plainStorageQuestionnaireAccessor,
            IPlainStorageAccessor<InterviewModel> plainStorageInterviewAccessor)
        {
            this.plainStorageQuestionnaireAccessor = plainStorageQuestionnaireAccessor;
            this.plainStorageInterviewAccessor = plainStorageInterviewAccessor;
        }

        private readonly Dictionary<Type, Func<Guid, InterviewModel, QuestionnaireDocument, MvxViewModel>> mapQuestions = new Dictionary<Type, Func<Guid, InterviewModel, QuestionnaireDocument, MvxViewModel>>()
        {
            { typeof(IGroup), (questionId, interview, questionnaire) => new GroupReferanceViewModel(questionId, interview, questionnaire) },
            { typeof(IStaticText), (questionId, interview, questionnaire) => new StaticTextViewModel(questionId, questionnaire) },
            { typeof(TextQuestion), (questionId, interview, questionnaire) => new TextQuestionViewModel(questionId, interview, questionnaire) },
        };

        public IEnumerable<object> Load(string interviewId, string chapterId)
        {
            var interview = plainStorageInterviewAccessor.GetById(interviewId);
            var questionaryId = interview.QuestionaryId.FormatGuid();

            var questionnaire = plainStorageQuestionnaireAccessor.GetById(questionaryId);
            
            Guid chapterIdGuid = new Guid(chapterId);
            var @group = questionnaire.Find<IGroup>(chapterIdGuid);
            if (chapterId != null && @group == null)
                throw new KeyNotFoundException("Grup with id : {0} don't found".FormatString(chapterId));

            List<object> entities = new List<object>();

            foreach (var child in @group.Children)
            {
                var entityType = child.GetType();
                var mapQuestionFunc = mapQuestions[entityType];
                var entityViewModel = mapQuestionFunc.Invoke(child.PublicKey, interview, questionnaire);

                entities.Add(entityViewModel);
            }

            return entities;
        }
    }
}