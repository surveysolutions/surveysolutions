using System;
using System.Globalization;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class DateTimeQuestionViewModel : MvxNotifyPropertyChanged, IInterviewEntityViewModel
    {
        private readonly ICommandService commandService;
        private readonly IPrincipal principal;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefullInterviewRepository interviewRepository;

        private Identity entityIdentity;
        private string interviewId;

        public DateTimeQuestionViewModel(ICommandService commandService,
            IPrincipal principal,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefullInterviewRepository interviewRepository,
            QuestionHeaderViewModel questionHeaderViewModel,
            ValidityViewModel validity,
            EnablementViewModel enablement,
            CommentsViewModel comments)
        {
            this.commandService = commandService;
            this.principal = principal;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;

            this.Header = questionHeaderViewModel;
            this.Validity = validity;
            this.Enablement = enablement;
            this.Comments = comments;
        }

        public void Init(string interviewId, Identity entityIdentity)
        {
            this.Header.Init(interviewId, entityIdentity);
            this.Validity.Init(interviewId, entityIdentity);
            this.Comments.Init(interviewId, entityIdentity);
            this.Enablement.Init(interviewId, entityIdentity);

            this.entityIdentity = entityIdentity;
            this.interviewId = interviewId;

            var interview = this.interviewRepository.Get(interviewId);
            var answerModel = interview.GetDateTimeAnswer(entityIdentity);
            if (answerModel != null && answerModel.IsAnswered)
            {
                SetToView(answerModel.Answer);
            }
        }

        public QuestionHeaderViewModel Header { get; private set; }
        public ValidityViewModel Validity { get; private set; }
        public EnablementViewModel Enablement { get; private set; }
        public CommentsViewModel Comments { get; private set; }

        public IMvxCommand AnswerCommand
        {
            get
            {
                return new MvxCommand<DateTime>(answerValue =>
                {
                    try
                    {
                        commandService.Execute(new AnswerDateTimeQuestionCommand(
                            interviewId: Guid.Parse(interviewId),
                            userId: principal.CurrentUserIdentity.UserId,
                            questionId: this.entityIdentity.Id,
                            rosterVector: this.entityIdentity.RosterVector,
                            answerTime: DateTime.UtcNow,
                            answer: answerValue
                            ));
                        SetToView(answerValue);
                        Validity.ExecutedWithoutExceptions();
                    }
                    catch (Exception ex)
                    {
                        Validity.ProcessException(ex);
                    }
                });
            }
        }

        private void SetToView(DateTime answerValue)
        {
            Answer = answerValue.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);
        }

        private string answer;
        public string Answer
        {
            get { return answer; }
            set { answer = value; RaisePropertyChanged(); }
        }
    }
}