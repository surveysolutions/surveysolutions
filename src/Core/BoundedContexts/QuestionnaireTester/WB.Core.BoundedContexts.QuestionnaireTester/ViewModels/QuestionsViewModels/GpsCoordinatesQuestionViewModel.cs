using System;
using Cirrious.MvvmCross.Plugins.Location;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class GpsCoordinatesQuestionViewModel : MvxNotifyPropertyChanged, IInterviewEntityViewModel
    {
        public QuestionHeaderViewModel Header { get; set; }

        private bool isInProgress;
        public bool IsInProgress
        {
            get { return isInProgress; }
            set { isInProgress = value; RaisePropertyChanged(); }
        }

        private MvxCoordinates answer;
        public MvxCoordinates Answer
        {
            get { return answer; }
            set { answer = value; RaisePropertyChanged(); }
        }

        private string coordinates;
        public string Coordinates
        {
            get { return coordinates; }
            set { coordinates = value; RaisePropertyChanged(); }
        }

        private readonly ICommandService commandService;
        private readonly IUserIdentity userIdentity;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IMvxLocationWatcher geoLocationWatcher;

        private Identity questionIdentity;
        private Guid interviewId;

        public GpsCoordinatesQuestionViewModel(ICommandService commandService, 
            IUserIdentity userIdentity,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            IMvxLocationWatcher geoLocationWatcher,
            QuestionHeaderViewModel questionHeaderViewModel)
        {
            this.commandService = commandService;
            this.userIdentity = userIdentity;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.geoLocationWatcher = geoLocationWatcher;

            this.Header = questionHeaderViewModel;
        }

        public void Init(string interviewId, Identity entityIdentity)
        {
            if(interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            var interview = this.interviewRepository.GetById(interviewId);
            
            this.questionIdentity = entityIdentity;
            this.interviewId = interview.Id;

            this.Header.Init(interviewId, entityIdentity);

            var answerModel = interview.GetGpsCoordinatesAnswerModel(entityIdentity);
            if (answerModel != null)
            {
                this.Answer = new MvxCoordinates()
                {
                    Altitude = answerModel.Altitude,
                    Longitude = answerModel.Longitude,
                    Latitude = answerModel.Latitude,
                    Accuracy = answerModel.Accuracy
                };
            }
        }

        private IMvxCommand saveAnswerCommand;
        public IMvxCommand SaveAnswerCommand
        {
            get { return saveAnswerCommand ?? (saveAnswerCommand = new MvxCommand(SaveAnswer)); }
        }

        private IMvxCommand removeAnswerCommand;
        public IMvxCommand RemoveAnswerCommand
        {
            get { return removeAnswerCommand ?? (removeAnswerCommand = new MvxCommand(RemoveAnswer)); }
        }

        private void RemoveAnswer()
        {
            throw new NotImplementedException();
        }

        private void SaveAnswer()
        {
            this.IsInProgress = true;
            this.geoLocationWatcher.Start(options: new MvxLocationOptions(), success: (location) =>
            {
                this.geoLocationWatcher.Stop();
                this.IsInProgress = false;
                this.commandService.Execute(new AnswerGeoLocationQuestionCommand(
                 interviewId: interviewId,
                 userId: userIdentity.UserId,
                 questionId: this.questionIdentity.Id,
                 rosterVector: this.questionIdentity.RosterVector,
                 answerTime: DateTime.UtcNow,
                 accuracy: location.Coordinates.Accuracy ?? 0,
                 altitude: location.Coordinates.Altitude ?? 0,
                 latitude: location.Coordinates.Latitude,
                 longitude: location.Coordinates.Longitude,
                 timestamp: location.Timestamp));
                this.Answer = location.Coordinates;
                this.Coordinates = string.Format("{0}, {1}", location.Coordinates.Latitude,
                    location.Coordinates.Longitude);
            }, error: (error) =>
            {
                this.IsInProgress = false;
            });
        }
    }
}