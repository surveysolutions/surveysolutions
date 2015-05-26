using System;
using System.IO;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.PictureChooser;
using Cirrious.MvvmCross.ViewModels;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class MultimedaQuestionViewModel : MvxNotifyPropertyChanged, IInterviewEntityViewModel
    {
        private readonly IUserIdentity userIdentity;
        private Guid interviewId;
        private Identity questionIdentity;

        public MultimedaQuestionViewModel(
            IUserIdentity userIdentity,
            QuestionStateViewModel<PictureQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering)
        {
            this.userIdentity = userIdentity;
            this.QuestionState = questionStateViewModel;
            this.Answering = answering;
        }

        public AnsweringViewModel Answering { get; private set; }

        public QuestionStateViewModel<PictureQuestionAnswered> QuestionState { get; private set; }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.interviewId = Guid.Parse(interviewId);
            this.questionIdentity = entityIdentity;

            QuestionState.Init(interviewId, entityIdentity, navigationState);
        }

        public IMvxCommand RequestAnswerCommand
        {
            get
            {
                return new MvxCommand(async () =>
                {
                    var pictureChooserTask = Mvx.Resolve<IMvxPictureChooserTask>();
                    Stream picture = await pictureChooserTask.TakePictureAsync(700, 90);
                    var command = new AnswerPictureQuestionCommand(
                        interviewId,
                        userIdentity.UserId,
                        this.questionIdentity.Id,
                        this.questionIdentity.RosterVector,
                        DateTime.UtcNow, 
                        "");
                });
            }
        }
    }
}