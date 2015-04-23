using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;
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

        public void Init(string interviewId, Identity chapterIdentity)
        {
            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.Get(interview.QuestionnaireId);

            this.QuestionnaireTitle = questionnaire.Title;
            this.PrefilledQuestions = questionnaire.PrefilledQuestionsIds
                .Select(referenceToQuestion => new 
                {
                    InterviewId = interviewId,
                    Question = questionnaire.Questions[referenceToQuestion.Id].Title,
                    AnswerModel = GetAnswerModel(interview, referenceToQuestion)
                })
                .ToList();

        }

        private static AbstractInterviewAnswerModel GetAnswerModel(InterviewModel interview, QuestionnaireReferenceModel referenceToQuestion)
        {
            var identityAsString = ConversionHelper.ConvertIdAndRosterVectorToString(referenceToQuestion.Id, new decimal[0]);
            return interview.Answers.ContainsKey(identityAsString) ? interview.Answers[identityAsString] : null;
        }
    }
}