using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewInfrastructure;
using StaticTextViewModel = WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels.StaticTextViewModel;


namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModelLoader.Implimentation
{
    internal class InterviewInterviewStateFullViewModelFactory : IInterviewStateFullViewModelFactory
    {
        private readonly IPlainStorageAccessor<IQuestionnaireDocument> plainStorageQuestionnaireAccessor;
        private readonly IPlainStorageAccessor<InterviewStateFull> plainStorageInterviewAccessor;

        public InterviewInterviewStateFullViewModelFactory(
            IPlainStorageAccessor<IQuestionnaireDocument> plainStorageQuestionnaireAccessor,
            IPlainStorageAccessor<InterviewStateFull> plainStorageInterviewAccessor)
        {
            this.plainStorageQuestionnaireAccessor = plainStorageQuestionnaireAccessor;
            this.plainStorageInterviewAccessor = plainStorageInterviewAccessor;
        }

        private readonly Dictionary<Type, Func<IComposite, object, object>> mapQuestions = new Dictionary<Type, Func<IComposite, object, object>>()
        {
            { typeof(IGroup), GetGroupViewModel },
            { typeof(IStaticText), GetStaticTextViewModel },
            { typeof(TextQuestion), GetTextQuestionViewModel },
        };

        public IEnumerable<object> Load(string interviewId, string chapterId)
        {
            var interviewStateFull = plainStorageInterviewAccessor.GetById(interviewId);
            var questionaryId = interviewStateFull.QuestionaryId.FormatGuid();

            var questionnaire = plainStorageQuestionnaireAccessor.GetById(questionaryId);
            
            Guid chapterIdGuid = new Guid(chapterId);
            var @group = questionnaire.Find<IGroup>(chapterIdGuid);
            if (chapterId != null && @group == null)
                throw new KeyNotFoundException("Grup with id : {0} don't found".FormatString(chapterId));

            List<object> entities = new List<object>();

            foreach (var child in @group.Children)
            {
                var answerOnQuestion = interviewStateFull.GetAnswerOnQuestion(child.PublicKey);

                var entityType = child.GetType();

                var mapQuestionFunc = mapQuestions[entityType];
                var entityViewModel = mapQuestionFunc.Invoke(child, answerOnQuestion);

                entities.Add(entityViewModel);
            }

            return entities;
        }

        private static object GetTextQuestionViewModel(IComposite entity, object answer)
        {
            TextQuestion textQuestion = (TextQuestion)entity;
            TextQuestionViewModel textQuestionViewModel = new TextQuestionViewModel();
            textQuestionViewModel.Title = textQuestion.QuestionText;
            textQuestionViewModel.Answer = (string)answer;
            return textQuestion;

        }

        private static object GetStaticTextViewModel(IComposite entity, object answer)
        {
            IStaticText staticText = (IStaticText)entity;
            StaticTextViewModel staticTextViewModel = new StaticTextViewModel();
            staticTextViewModel.Title = staticText.Text;
            return staticText;
        }

        private static object GetGroupViewModel(IComposite entity, object answer)
        {
            IGroup @group = (IGroup) entity;
            GroupViewModel groupViewModel = new GroupViewModel();
            groupViewModel.Title = @group.Title;
            return @group;
        }

    }
}