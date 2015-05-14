using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class DateTimeQuestionViewModel : MvxNotifyPropertyChanged, IInterviewEntityViewModel
    {
        private readonly ICommandService commandService;
        private readonly IPrincipal principal;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefullInterviewRepository interviewRepository;

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
        }

        public QuestionHeaderViewModel Header { get; private set; }
        public ValidityViewModel Validity { get; private set; }
        public EnablementViewModel Enablement { get; private set; }
        public CommentsViewModel Comments { get; private set; }
    }
}