using System;
using System.Collections;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class InterviewLeftSidePanelViewModel : MvxNotifyPropertyChanged
    {
        private readonly IPlainRepository<QuestionnaireModel> questionnaireRepository;
        private readonly IPlainRepository<InterviewModel> interviewRepository;

        public string QuestionnaireTitle { get; set; }
        public IEnumerable PrefilledQuestions { get; set; } 

        public InterviewLeftSidePanelViewModel(IPlainRepository<QuestionnaireModel> questionnaireRepository,
             IPlainRepository<InterviewModel> interviewRepository)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
        }

        public void Init(string interviewId, string chapterId)
        {
            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.Get(interview.QuestionnaireId.FormatGuid());

            this.QuestionnaireTitle = questionnaire.Title;
            this.PrefilledQuestions = questionnaire.PrefilledQuestionsIds
                .Select(questionId => new PrefilledQuestion
                {
                    Question = questionnaire.Questions[questionId].Title,
                    Answer = "some answer"
                })
                .ToList();

        }

        public class PrefilledQuestion
        {
            public string Question { get; set; }
            public string Answer { get; set; }
        }
    }
}