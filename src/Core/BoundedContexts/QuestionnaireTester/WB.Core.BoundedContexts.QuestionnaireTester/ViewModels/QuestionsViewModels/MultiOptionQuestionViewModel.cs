using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities.QuestionModels;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class MultiOptionQuestionViewModel : MvxNotifyPropertyChanged, IInterviewEntityViewModel
    {
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefullInterviewRepository interviewRepository;

        public MultiOptionQuestionViewModel(QuestionHeaderViewModel questionHeaderViewModel,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository, 
            IStatefullInterviewRepository interviewRepository)
        {
            this.Options = new ReadOnlyCollection<QuestionOptionViewModel>(new List<QuestionOptionViewModel>());
            this.Header = questionHeaderViewModel;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
        }

        public void Init(string interviewId, Identity entityIdentity)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.Header.Init(interviewId, entityIdentity);

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);
            var questionModel = (MultiOptionQuestionModel)questionnaire.Questions[entityIdentity.Id];

            this.Options = new ReadOnlyCollection<QuestionOptionViewModel>(questionModel.Options.Select(this.ToViewModel).ToList());
        }

        public QuestionHeaderViewModel Header { get; private set; }

        public ReadOnlyCollection<QuestionOptionViewModel> Options { get; private set; }

        private QuestionOptionViewModel ToViewModel(OptionModel model)
        {
            return new QuestionOptionViewModel
            {
                Value = model.Value,
                Title = model.Title
            };
        }
    }
}