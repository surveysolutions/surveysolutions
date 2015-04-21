using System;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities.QuestionModels;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class MultiOptionQuestionViewModel : BaseInterviewItemViewModel
    {
        private readonly ICommandService commandService;
        private readonly IPrincipal principal;
        private Identity identity;
        private InterviewModel interviewModel;
        private QuestionnaireModel questionnaireModel;

        public MultiOptionQuestionViewModel(ICommandService commandService, IPrincipal principal)
        {
            this.commandService = commandService;
            this.principal = principal;
        }

        public override void Init(Identity identity, InterviewModel interviewModel, QuestionnaireModel questionnaireModel)
        {
            if (identity == null) throw new ArgumentNullException("identity");
            if (interviewModel == null) throw new ArgumentNullException("interviewModel");
            if (questionnaireModel == null) throw new ArgumentNullException("questionnaireModel");

            this.identity = identity;
            this.interviewModel = interviewModel;
            this.questionnaireModel = questionnaireModel;

            var questionModel = (MultiOptionQuestionModel)this.questionnaireModel.Questions[this.identity.Id];
            var answerModel = this.interviewModel.GetMultiOptionAnswerModel(this.identity);

            Title = questionModel.Title;

            if (answerModel != null)
            {
                Answer = answerModel.Answers;
            }
        }

        private string title;
        public string Title
        {
            get { return title; }
            set { title = value; RaisePropertyChanged(() => Title); }
        }

        private decimal[] answer;
        public decimal[] Answer
        {
            get { return answer; }
            set { answer = value; RaisePropertyChanged(() => Answer); }
        }
    }
}