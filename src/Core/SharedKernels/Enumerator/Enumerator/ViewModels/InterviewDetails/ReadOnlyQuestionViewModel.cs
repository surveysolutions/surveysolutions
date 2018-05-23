using System;
using System.Globalization;
using Main.Core.Entities.SubEntities;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class ReadOnlyQuestionViewModel :
        MvxNotifyPropertyChanged,
        IInterviewEntityViewModel
    {
        public Identity Identity { get; private set; }

        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IQuestionnaireStorage questionnaireRepository;

        public ReadOnlyQuestionViewModel(
            IStatefulInterviewRepository interviewRepository,
            IQuestionnaireStorage questionnaireRepository,
            DynamicTextViewModel dynamicTextViewModel)
        {
            this.interviewRepository = interviewRepository;
            this.questionnaireRepository = questionnaireRepository;
            this.Title = dynamicTextViewModel;
        }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));

            var interview = this.interviewRepository.Get(interviewId);

            this.Identity = entityIdentity;
            this.Title.Init(interviewId, entityIdentity);

            var question = interview.GetQuestion(entityIdentity);
            if (question.IsAnswered())
            {
                var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
                var questionType = questionnaire.GetQuestionType(entityIdentity.Id);
                this.Answer = questionType == QuestionType.GpsCoordinates
                    ? $"{question.GetAsInterviewTreeGpsQuestion().GetAnswer().Value.Latitude}, {question.GetAsInterviewTreeGpsQuestion().GetAnswer().Value.Longitude}"
                    : question.GetAnswerAsString(CultureInfo.CurrentUICulture);
            }
        }

        public DynamicTextViewModel Title { get; private set; }

        public string Answer { get; private set; }
    }
}