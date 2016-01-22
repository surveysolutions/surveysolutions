using System;
using System.IO;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MvvmCross.Plugins.PictureChooser;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using Identity = WB.Core.SharedKernels.DataCollection.Identity;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class MultimedaQuestionViewModel : MvxNotifyPropertyChanged, 
        IInterviewEntityViewModel,
        ILiteEventHandler<AnswerRemoved>,
        IDisposable
    {
        private readonly Guid userId;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireStorage;
        private readonly IPlainInterviewFileStorage plainInterviewFileStorage;
        private readonly ILiteEventRegistry eventRegistry;
        private Guid interviewId;
        private Identity questionIdentity;
        private string variableName;
        private byte[] answer;

        public MultimedaQuestionViewModel(
            IPrincipal principal,
            IStatefulInterviewRepository interviewRepository,
            IPlainInterviewFileStorage plainInterviewFileStorage,
            ILiteEventRegistry eventRegistry,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireStorage,
            QuestionStateViewModel<PictureQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering)
        {
            this.userId = principal.CurrentUserIdentity.UserId;
            this.interviewRepository = interviewRepository;
            this.plainInterviewFileStorage = plainInterviewFileStorage;
            this.eventRegistry = eventRegistry;
            this.questionnaireStorage = questionnaireStorage;
            this.QuestionState = questionStateViewModel;
            this.Answering = answering;
        }

        public AnsweringViewModel Answering { get; private set; }

        public QuestionStateViewModel<PictureQuestionAnswered> QuestionState { get; private set; }

        public byte[] Answer
        {
            get { return this.answer; }
            set
            {
                this.answer = value;
                this.RaisePropertyChanged();
            }
        }

        public Identity Identity { get { return this.questionIdentity; } }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.QuestionState.Init(interviewId, entityIdentity, navigationState);
            this.questionIdentity = entityIdentity;

            IStatefulInterview interview = this.interviewRepository.Get(interviewId);
            this.interviewId = interview.Id;

            QuestionnaireModel questionnaire = this.questionnaireStorage.GetById(interview.QuestionnaireId);
            this.variableName = questionnaire.GetMultimediaQuestion(entityIdentity.Id).Variable;
            MultimediaAnswer multimediaAnswer = interview.GetMultimediaAnswer(entityIdentity);
            if (multimediaAnswer.IsAnswered)
            {
                this.Answer = this.plainInterviewFileStorage.GetInterviewBinaryData(this.interviewId, multimediaAnswer.PictureFileName);
            }

            this.eventRegistry.Subscribe(this, interviewId);
        }

        public IMvxCommand RequestAnswerCommand
        {
            get
            {
                return new MvxCommand(async () =>
                {
                    var pictureFileName = this.GetPictureFileName();

                    var pictureChooserTask = Mvx.Resolve<IMvxPictureChooserTask>();
                    using (Stream pictureStream = await pictureChooserTask.TakePictureAsync(400, 95))
                    {
                        if (pictureStream != null)
                        {
                            this.StorePictureFile(pictureStream, pictureFileName);

                            var command = new AnswerPictureQuestionCommand(
                                this.interviewId,
                                this.userId,
                                this.questionIdentity.Id,
                                this.questionIdentity.RosterVector,
                                DateTime.UtcNow,
                                pictureFileName);

                            try
                            {
                                await this.Answering.SendAnswerQuestionCommandAsync(command);
                                this.Answer = this.plainInterviewFileStorage.GetInterviewBinaryData(this.interviewId, pictureFileName);
                                this.QuestionState.Validity.ExecutedWithoutExceptions();
                            }
                            catch (InterviewException ex)
                            {
                                this.plainInterviewFileStorage.RemoveInterviewBinaryData(this.interviewId, pictureFileName);
                                this.QuestionState.Validity.ProcessException(ex);
                            }
                        }
                    }
                });
            }
        }

        public IMvxCommand RemoveAnswerCommand
        {
            get
            {
                return new MvxCommand(async () =>
                {
                    try
                    {
                        await this.Answering.SendRemoveAnswerCommandAsync(
                            new RemoveAnswerCommand(this.interviewId,
                                this.userId, 
                                this.questionIdentity,
                                DateTime.UtcNow));
                        this.QuestionState.Validity.ExecutedWithoutExceptions();
                    }
                    catch (InterviewException exception)
                    {
                        this.QuestionState.Validity.ProcessException(exception);
                    }
                });
            }
        }

        public void Handle(AnswerRemoved answerRemoved)
        {
            var myAnswerRemoved = this.questionIdentity.Id == answerRemoved.QuestionId && 
                                  this.questionIdentity.RosterVector.Identical(answerRemoved.RosterVector);
            if (myAnswerRemoved)
            {
                this.Answer = null;
                this.QuestionState.IsAnswered = false;
                this.plainInterviewFileStorage.RemoveInterviewBinaryData(this.interviewId, this.GetPictureFileName());
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

        private string GetPictureFileName()
        {
            return String.Format("{0}{1}.jpg", this.variableName, string.Join("-", this.questionIdentity.RosterVector));
        }

        public void Dispose()
        {
            this.eventRegistry.Unsubscribe(this, interviewId.FormatGuid());
            this.QuestionState.Dispose();
        }
    }
}