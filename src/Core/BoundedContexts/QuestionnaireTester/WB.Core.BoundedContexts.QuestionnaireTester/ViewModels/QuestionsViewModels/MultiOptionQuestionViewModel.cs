using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities.QuestionModels;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class MultiOptionQuestionViewModel : MvxNotifyPropertyChanged, IInterviewEntityViewModel
    {
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefullInterviewRepository interviewRepository;
        private readonly ICommandService commandService;
        private readonly IPrincipal principal;
        private Guid interviewId;
        private Identity questionIdentity;
        private Guid userId;
        private int? maxAllowedAnswers;

        public MultiOptionQuestionViewModel(
            QuestionHeaderViewModel questionHeaderViewModel,
            EnablementViewModel enablementViewModel,
            ValidityViewModel validityViewModel,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository, 
            ICommandService commandService,
            IStatefullInterviewRepository interviewRepository,
            IPrincipal principal)
        {
            this.Options = new ReadOnlyCollection<MultiOptionQuestionOptionViewModel>(new List<MultiOptionQuestionOptionViewModel>());
            this.Header = questionHeaderViewModel;
            this.Enablement = enablementViewModel;
            this.Validity = validityViewModel;
            this.questionnaireRepository = questionnaireRepository;
            this.commandService = commandService;
            this.principal = principal;
            this.interviewRepository = interviewRepository;
        }

        public void Init(string interviewId, Identity entityIdentity)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.Header.Init(interviewId, entityIdentity);
            this.Enablement.Init(interviewId, entityIdentity);
            this.Validity.Init(interviewId, entityIdentity);

            this.questionIdentity = entityIdentity;

            this.userId = principal.CurrentUserIdentity.UserId;
            var interview = this.interviewRepository.Get(interviewId);
            this.interviewId = interview.Id;
            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);

            var questionModel = (MultiOptionQuestionModel)questionnaire.Questions[entityIdentity.Id];
            maxAllowedAnswers = questionModel.MaxAllowedAnswers;

            this.Options = new ReadOnlyCollection<MultiOptionQuestionOptionViewModel>(questionModel.Options.Select(this.ToViewModel).ToList());
        }

        public QuestionHeaderViewModel Header { get; private set; }

        public EnablementViewModel Enablement { get; private set; }

        public ValidityViewModel Validity { get; private set; }

        public ReadOnlyCollection<MultiOptionQuestionOptionViewModel> Options { get; private set; }

        private MultiOptionQuestionOptionViewModel ToViewModel(OptionModel model)
        {
            var result = new MultiOptionQuestionOptionViewModel
            {
                Value = model.Value,
                Title = model.Title
            };
            result.BeforeCheckedChanged += BeforeChecked;

            return result;
        }

        void BeforeChecked(object sender, OptionCheckedArgs args)
        {
            var allSelectedOptions = this.Options.Where(x => x.Checked);
            if (maxAllowedAnswers.HasValue && allSelectedOptions.Count() >= maxAllowedAnswers)
            {
                args.CancelCheck = true;
                return;
            }

            var changedOption = (MultiOptionQuestionOptionViewModel) sender;

            if (args.NewValue)
            {
                allSelectedOptions = allSelectedOptions.Concat(changedOption.ToEnumerable());
            }
            else
            {
                allSelectedOptions = allSelectedOptions.Except(changedOption.ToEnumerable());
            }

            var selectedValues = allSelectedOptions.Select(x => x.Value).ToArray();
            var command = new AnswerMultipleOptionsQuestionCommand(
                this.interviewId,
                this.userId,
                this.questionIdentity.Id,
                this.questionIdentity.RosterVector,
                DateTime.UtcNow,
                selectedValues);
            try
            {
                this.commandService.Execute(command);
                Validity.RemoveExceptionFlag();
            }
            catch (Exception e)
            {
                args.CancelCheck = true;
                Validity.AddExceptionFlag(e);
            }
        }
    }
}