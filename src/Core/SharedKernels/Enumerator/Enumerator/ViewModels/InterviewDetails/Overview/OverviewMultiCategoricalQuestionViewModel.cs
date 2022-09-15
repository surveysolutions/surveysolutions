using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Views.Interview.Overview;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Overview;

public class OverviewMultiCategoricalQuestionViewModel : OverviewQuestionViewModel
{
    public class OverviewMultiCategoricalQuestionAnswer : IDisposable
    {
        public OverviewMultiCategoricalQuestionAnswer(AttachmentViewModel attachment)
        {
            Attachment = attachment;
        }

        public string Answer { get; set; }
        public AttachmentViewModel Attachment { get; set; }
        public OverviewNodeState State { get; set; }

        public void Dispose()
        {
            Attachment?.ViewDestroy();
            Attachment?.Dispose();
        }
    }

    public OverviewMultiCategoricalQuestionViewModel(InterviewTreeQuestion treeQuestion, IStatefulInterview interview, 
        IUserInteractionService userInteractionService, IInterviewViewModelFactory interviewViewModelFactory,
        IQuestionnaire questionnaire) 
        : base(treeQuestion, interview, userInteractionService)
    {
        Answers = new List<OverviewMultiCategoricalQuestionAnswer>();

        if (treeQuestion.IsAnswered())
        {
            if (treeQuestion.IsMultiFixedOption)
            {
                var multiOptionQuestion = treeQuestion.GetAsInterviewTreeMultiOptionQuestion();
                var selectedValues = multiOptionQuestion.GetAnswer().CheckedValues;

                foreach (var selectedValue in selectedValues)
                {
                    var item = interviewViewModelFactory.GetNew<OverviewMultiCategoricalQuestionAnswer>();
                    item.Answer = questionnaire.GetAnswerOptionTitle(treeQuestion.Identity.Id, selectedValue, null);
                    var attachmentName = interview.GetAttachmentForEntityOption(treeQuestion.Identity, selectedValue, null);
                    item.Attachment.InitAsStatic(treeQuestion.Tree.InterviewId, attachmentName);
                    item.State = this.State;
                    Answers.Add(item);
                }
            }

            if (treeQuestion.IsYesNo)
            {
                var yesNoQuestion = treeQuestion.GetAsInterviewTreeYesNoQuestion();
                var yesNoAnswerOptions = yesNoQuestion.GetAnswer().CheckedOptions;
                foreach (var yesNoOption in yesNoAnswerOptions)
                {
                    var item = interviewViewModelFactory.GetNew<OverviewMultiCategoricalQuestionAnswer>();
                    var optionTitle = questionnaire.GetAnswerOptionTitle(treeQuestion.Identity.Id, yesNoOption.Value, null);
                    var titleTypeAnswer = yesNoOption.Yes ? UIResources.Yes : UIResources.No;
                    item.Answer = $"{optionTitle} ({titleTypeAnswer})";
                    var attachmentName = interview.GetAttachmentForEntityOption(treeQuestion.Identity, yesNoOption.Value, null);
                    item.Attachment.InitAsStatic(treeQuestion.Tree.InterviewId, attachmentName);
                    item.State = this.State;
                    Answers.Add(item);
                }
            }
        }
    }
    public List<OverviewMultiCategoricalQuestionAnswer> Answers { get; set; }
}
