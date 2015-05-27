using System;
using System.IO;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.PictureChooser;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using Identity = WB.Core.SharedKernels.DataCollection.Identity;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class MultimedaQuestionViewModel : MvxNotifyPropertyChanged, IInterviewEntityViewModel
    {
        private readonly IUserIdentity userIdentity;
        private readonly IStatefullInterviewRepository interviewRepository;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireStorage;
        private readonly IPlainInterviewFileStorage plainInterviewFileStorage;
        private Guid interviewId;
        private Identity questionIdentity;
        private string variableName;
        private byte[] answer;

        public MultimedaQuestionViewModel(
            IUserIdentity userIdentity,
            IStatefullInterviewRepository interviewRepository,
            IPlainInterviewFileStorage plainInterviewFileStorage,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireStorage,
            QuestionStateViewModel<PictureQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering)
        {
            this.userIdentity = userIdentity;
            this.interviewRepository = interviewRepository;
            this.plainInterviewFileStorage = plainInterviewFileStorage;
            this.questionnaireStorage = questionnaireStorage;
            this.QuestionState = questionStateViewModel;
            this.Answering = answering;
        }

        public AnsweringViewModel Answering { get; private set; }

        public QuestionStateViewModel<PictureQuestionAnswered> QuestionState { get; private set; }

        public byte[] Answer
        {
            get { return this.answer; }
            set { this.answer = value; this.RaisePropertyChanged(); }
        }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.QuestionState.Init(interviewId, entityIdentity, navigationState);
            this.questionIdentity = entityIdentity;

            IStatefulInterview interview = this.interviewRepository.Get(interviewId);
            this.interviewId = interview.Id;

            QuestionnaireModel questionnaire = this.questionnaireStorage.GetById(interview.QuestionnaireId);
            this.variableName = questionnaire.Questions[entityIdentity.Id].Variable;
            MultimediaAnswer multimediaAnswer = interview.GetMultimediaAnswer(entityIdentity);
            if (multimediaAnswer != null && multimediaAnswer.IsAnswered)
            {
                this.Answer = this.plainInterviewFileStorage.GetInterviewBinaryData(this.interviewId, multimediaAnswer.PictureFileName);
            }
        }

        public IMvxCommand RequestAnswerCommand
        {
            get
            {
                return new MvxCommand(async () =>
                {
                    var pictureFileName = String.Format("{0}{1}.jpg", this.variableName, string.Join("-", this.questionIdentity.RosterVector));

                    var pictureChooserTask = Mvx.Resolve<IMvxPictureChooserTask>();
                    Stream pictureStream = await pictureChooserTask.TakePictureAsync(400, 95);
                    if (pictureStream != null)
                    {
                        this.StorePictureFile(pictureStream, pictureFileName);

                        var command = new AnswerPictureQuestionCommand(
                            interviewId,
                            userIdentity.UserId,
                            this.questionIdentity.Id,
                            this.questionIdentity.RosterVector,
                            DateTime.UtcNow,
                            pictureFileName);

                        try
                        {
                            await this.Answering.SendAnswerQuestionCommand(command);
                            this.Answer = this.plainInterviewFileStorage.GetInterviewBinaryData(this.interviewId, pictureFileName);
                            QuestionState.ExecutedAnswerCommandWithoutExceptions();
                        }
                        catch (InterviewException ex)
                        {
                            this.plainInterviewFileStorage.RemoveInterviewBinaryData(this.interviewId, pictureFileName);
                            QuestionState.ProcessAnswerCommandException(ex);
                        }
                    }
                });
            }
        }

        private void StorePictureFile(Stream pictureStream, string pictureFileName)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                pictureStream.CopyTo(ms);
                byte[] pictureBytes = ms.ToArray();
                this.plainInterviewFileStorage.StoreInterviewBinaryData(this.interviewId, pictureFileName, pictureBytes);
            }
        }
    }
}