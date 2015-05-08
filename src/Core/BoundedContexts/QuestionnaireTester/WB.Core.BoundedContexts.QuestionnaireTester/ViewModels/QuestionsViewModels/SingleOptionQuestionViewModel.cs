using System;
using System.Collections.Generic;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities.QuestionModels;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class SingleOptionQuestionViewModel : MvxNotifyPropertyChanged, IInterviewEntityViewModel
    {
        private readonly ICommandService commandService;
        private readonly Guid userId;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefullInterviewRepository interviewRepository;

        public SingleOptionQuestionViewModel(
            ICommandService commandService,
            IPrincipal principal,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefullInterviewRepository interviewRepository,
            QuestionHeaderViewModel questionHeaderViewModel,
            EnablementViewModel enablementViewModel)
        {
            if (commandService == null) throw new ArgumentNullException("commandService");
            if (principal == null) throw new ArgumentNullException("principal");
            if (questionnaireRepository == null) throw new ArgumentNullException("questionnaireRepository");
            if (interviewRepository == null) throw new ArgumentNullException("interviewRepository");

            this.commandService = commandService;
            this.userId = principal.CurrentUserIdentity.UserId;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;

            this.Header = questionHeaderViewModel;
            this.Enablement = enablementViewModel;
        }

        private Identity questionIdentity;
        private Guid interviewId;

        public QuestionHeaderViewModel Header { get; private set; }
        public EnablementViewModel Enablement { get; private set; }
        public IList<SigleOptionQuestionOptionViewModel> Options { get; private set; }
        private SigleOptionQuestionOptionViewModel selectedOption;

        public void Init(string interviewId, Identity entityIdentity)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.Header.Init(interviewId, entityIdentity);
            this.Enablement.Init(interviewId, entityIdentity);

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);
            var questionModel = (SingleOptionQuestionModel)questionnaire.Questions[entityIdentity.Id];
            var answerModel = interview.GetSingleOptionAnswerModel(entityIdentity);

            this.questionIdentity = entityIdentity;
            this.interviewId = interview.Id;

            this.Options = questionModel.Options.Select(this.ToViewModel).ToList();
            this.selectedOption = this.Options.SingleOrDefault(option => option.Value == Monads.Maybe(() => answerModel.Answer));
        }

        public SigleOptionQuestionOptionViewModel SelectedOption
        {
            get { return selectedOption; }

            set
            {
                try
                {
                    this.commandService.Execute(new AnswerSingleOptionQuestionCommand(
                        this.interviewId,
                        this.userId,
                        this.questionIdentity.Id,
                        this.questionIdentity.RosterVector,
                        DateTime.Now,
                        value.Value));

                    selectedOption = value;
                }
                finally
                {
                    this.RaisePropertyChanged();
                }
            }
        }

        private SigleOptionQuestionOptionViewModel ToViewModel(OptionModel model)
        {
            return new SigleOptionQuestionOptionViewModel
            {
                Value = model.Value,
                Title = model.Title,
                Enablement = this.Enablement,
            };
        }
    }
}