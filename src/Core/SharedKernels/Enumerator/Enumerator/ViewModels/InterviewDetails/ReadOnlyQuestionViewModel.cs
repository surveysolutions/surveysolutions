using System;
using System.Globalization;
using Main.Core.Entities.SubEntities;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class ReadOnlyQuestionViewModel :
        MvxNotifyPropertyChanged,
        IInterviewEntityViewModel,
        IDisposable
    {
        public Identity Identity { get; private set; }

        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IQuestionnaireStorage questionnaireRepository;

        public IQuestionStateViewModel QuestionState { get; }
        
        public ReadOnlyQuestionViewModel(
            IStatefulInterviewRepository interviewRepository,
            IQuestionnaireStorage questionnaireRepository,
            ReadonlyQuestionStateViewModel questionStateViewModelBase)
        {
            this.interviewRepository = interviewRepository;
            this.questionnaireRepository = questionnaireRepository;
            this.QuestionState = questionStateViewModelBase;
        }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));

            var interview = this.interviewRepository.Get(interviewId);

            this.Identity = entityIdentity;
            this.QuestionState.Init(interviewId, entityIdentity, navigationState);

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

        public string Answer { get; private set; }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            QuestionState?.Dispose();
        }
    }
}
