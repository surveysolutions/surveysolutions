using System;
using System.Collections.Generic;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
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
        public IList<SingleOptionQuestionOptionViewModel> Options { get; private set; }

        public void Init(string interviewId, Identity entityIdentity)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.Header.Init(interviewId, entityIdentity);
            this.Enablement.Init(interviewId, entityIdentity);

            this.Header.Enablement = Enablement;

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);
            var questionModel = (SingleOptionQuestionModel)questionnaire.Questions[entityIdentity.Id];
            var answerModel = interview.GetSingleOptionAnswerModel(entityIdentity);
            var selectedValue = Monads.Maybe(() => answerModel.Answer);

            this.questionIdentity = entityIdentity;
            this.interviewId = interview.Id;

            this.Options = questionModel
                .Options
                .Select(model => this.ToViewModel(model, isSelected: model.Value == selectedValue))
                .ToList();
        }

        private void OptionSelected(object sender, EventArgs eventArgs)
        {
            var selectedOption = (SingleOptionQuestionOptionViewModel) sender;

            this.commandService.Execute(new AnswerSingleOptionQuestionCommand(
                this.interviewId,
                this.userId,
                this.questionIdentity.Id,
                this.questionIdentity.RosterVector,
                DateTime.Now,
                selectedOption.Value));

            var optionsToUnselect = this.Options.Where(option => option != selectedOption && option.Selected);

            foreach (var optionToUnselect in optionsToUnselect)
            {
                optionToUnselect.Selected = false;
            }
        }

        private SingleOptionQuestionOptionViewModel ToViewModel(OptionModel model, bool isSelected)
        {
            var optionViewModel = new SingleOptionQuestionOptionViewModel
            {
                Enablement = this.Enablement,

                Value = model.Value,
                Title = model.Title,
                Selected = isSelected,
            };

            optionViewModel.BeforeSelected += OptionSelected;

            return optionViewModel;
        }
    }
}