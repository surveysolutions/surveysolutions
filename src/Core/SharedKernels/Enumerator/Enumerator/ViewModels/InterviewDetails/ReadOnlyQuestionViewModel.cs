using System;
using System.Globalization;
using System.Linq;
using Main.Core.Entities.SubEntities;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;
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

        public AttachmentViewModel Attachment { get; }
        public IQuestionStateViewModel QuestionState { get; }
        public QuestionInstructionViewModel InstructionViewModel { get; private set; }

        public ReadOnlyQuestionViewModel(
            IStatefulInterviewRepository interviewRepository,
            IQuestionnaireStorage questionnaireRepository,
            AttachmentViewModel attachmentViewModel,
            ReadonlyQuestionStateViewModel questionStateViewModelBase,
            QuestionInstructionViewModel instructionViewModel)
        {
            this.interviewRepository = interviewRepository;
            this.questionnaireRepository = questionnaireRepository;
            this.QuestionState = questionStateViewModelBase;
            this.InstructionViewModel = instructionViewModel;
            this.Attachment = attachmentViewModel;
        }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));

            var interview = this.interviewRepository.Get(interviewId);

            this.Identity = entityIdentity;
            this.QuestionState.Init(interviewId, entityIdentity, navigationState);
            
            this.InstructionViewModel.Init(interviewId, entityIdentity, navigationState);

            var question = interview.GetQuestion(entityIdentity);
            if (question.IsAnswered())
            {
                var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
                var questionType = questionnaire.GetQuestionType(entityIdentity.Id);
                this.Answer = questionType == QuestionType.GpsCoordinates
                    ? $"{question.GetAsInterviewTreeGpsQuestion().GetAnswer().Value.Latitude}, {question.GetAsInterviewTreeGpsQuestion().GetAnswer().Value.Longitude}"
                    : question.GetAnswerAsString(CultureInfo.CurrentUICulture);
                
                InitAttachment(interviewId, entityIdentity, question, questionnaire);
            }
        }

        private void InitAttachment(string interviewId, Identity entityIdentity, InterviewTreeQuestion question,
            IQuestionnaire questionnaire)
        {
            decimal? optionValue = null;
            int? parentValue = null;

            if (question.IsInteger)
                optionValue = question.GetAsInterviewTreeIntegerQuestion().GetAnswer().Value;
            if (question.IsDouble)
                optionValue = Convert.ToDecimal(question.GetAsInterviewTreeDoubleQuestion().GetAnswer()?.Value);
            if (question.IsSingleFixedOption || question.IsCascading)
            {
                if (question.InterviewQuestion is InterviewTreeCascadingQuestion cascading)
                    parentValue = cascading.GetCascadingParentQuestion()?.GetAnswer()?.SelectedValue;

                optionValue = question.GetAsInterviewTreeSingleOptionQuestion().GetAnswer()?.SelectedValue;
            }

            if (optionValue.HasValue)
            {
                var attachmentName = questionnaire.GetAnswerOptionAttachment(entityIdentity.Id, optionValue.Value, parentValue);
                this.Attachment.InitAsStatic(interviewId, attachmentName);
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
