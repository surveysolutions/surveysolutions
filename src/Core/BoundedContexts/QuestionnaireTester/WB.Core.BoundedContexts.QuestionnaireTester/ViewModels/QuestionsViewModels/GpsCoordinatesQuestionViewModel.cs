using System;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities.QuestionModels;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class GpsCoordinatesQuestionViewModel : MvxNotifyPropertyChanged, IInterviewItemViewModel
    {
        private readonly ICommandService commandService;
        private readonly IUserIdentity userIdentity;
        private readonly IPlainRepository<QuestionnaireModel> questionnaireRepository;
        private readonly IPlainRepository<InterviewModel> interviewRepository;

        private Identity questionIdentity;
        private Guid interviewId;

        public GpsCoordinatesQuestionViewModel(ICommandService commandService, IUserIdentity userIdentity, IPlainRepository<QuestionnaireModel> questionnaireRepository,
             IPlainRepository<InterviewModel> interviewRepository)
        {
            this.commandService = commandService;
            this.userIdentity = userIdentity;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
        }

        public void Init(string interviewId, Identity questionIdentity)
        {
            if (questionIdentity == null) throw new ArgumentNullException("questionIdentity");

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.Get(interview.QuestionnaireId);

            GpsCoordinatesQuestionModel questionModel = (GpsCoordinatesQuestionModel)questionnaire.Questions[questionIdentity.Id];
            var answerModel = interview.GetGpsCoordinatesAnswerModel(questionIdentity);
            
            this.questionIdentity = questionIdentity;
            this.interviewId = interview.Id;

            this.Title = questionModel.Title;

            if (answerModel != null)
            {

            }
        }

        private IMvxCommand saveAnswerCommand;
        public IMvxCommand SaveAnswerCommand
        {
            get { return saveAnswerCommand ?? (saveAnswerCommand = new MvxCommand(SaveAnswer)); }
        }

        private string title;
        public string Title
        {
            get { return title; }
            set { title = value; RaisePropertyChanged(); }
        }

        private string answer;
        public string Answer
        {
            get { return answer; }
            set { answer = value; RaisePropertyChanged(); }
        }

        private void SaveAnswer()
        {
            this.commandService.Execute(new AnswerGeoLocationQuestionCommand(
                interviewId: interviewId,
                userId: userIdentity.UserId,
                questionId: this.questionIdentity.Id,
                rosterVector: this.questionIdentity.RosterVector,
                answerTime: DateTime.UtcNow, 
                accuracy: 0, 
                altitude: 0, 
                latitude: 0, 
                longitude: 0, 
                timestamp: DateTimeOffset.UtcNow));
        }
    }
}