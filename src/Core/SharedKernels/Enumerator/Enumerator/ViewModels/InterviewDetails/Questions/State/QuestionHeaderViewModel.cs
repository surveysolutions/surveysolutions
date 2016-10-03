using System;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State
{
    public class QuestionHeaderViewModel : MvxNotifyPropertyChanged,
        ICompositeEntity,
        IDisposable
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IQuestionnaireStorage questionnaireRepository;

        public event EventHandler ShowComments;

        public DynamicTextViewModel Title { get; }
        public EnablementViewModel Enablement { get; }

        public Identity Identity => this.questionIdentity;

        private Identity questionIdentity;
        public QuestionHeaderViewModel(
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            DynamicTextViewModel dynamicTextViewModel,
            EnablementViewModel enablementViewModel)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;

            this.Enablement = enablementViewModel;
            this.Title = dynamicTextViewModel;
        }

        public void Init(string interviewId, Identity questionIdentity)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (questionIdentity == null) throw new ArgumentNullException(nameof(questionIdentity));

            this.questionIdentity = questionIdentity;

            var interview = this.interviewRepository.Get(interviewId);
            IQuestionnaire questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
            
            this.Title.Init(interviewId, questionIdentity, questionnaire.GetQuestionTitle(questionIdentity.Id));
            this.Enablement.Init(interviewId, questionIdentity);
        }

        public ICommand ShowCommentEditorCommand => new MvxCommand(() => ShowComments?.Invoke(this, EventArgs.Empty));

        public void Dispose()
        {
            this.Title.Dispose();
            this.Enablement.Dispose();
        }
    }
}