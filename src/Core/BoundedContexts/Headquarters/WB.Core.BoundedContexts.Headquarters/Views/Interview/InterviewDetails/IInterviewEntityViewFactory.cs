using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public interface IInterviewEntityViewFactory
    {
        InterviewQuestionView BuildInterviewLinkedToRosterQuestionView(IQuestion question, 
            InterviewQuestion answeredQuestion,
            Dictionary<string, string> answersForTitleSubstitution,
            Dictionary<decimal[], string> availableOptions, 
            bool isParentGroupDisabled, 
            decimal[] rosterVector, 
            InterviewStatus interviewStatus);

        InterviewQuestionView BuildInterviewLinkedToListQuestionView(IQuestion question,
            InterviewQuestion answeredQuestion,
            Dictionary<string, string> answersForTitleSubstitution,
            object answerToListQuestion,
            bool isParentGroupDisabled,
            decimal[] rosterVector,
            InterviewStatus interviewStatus);

        InterviewQuestionView BuildInterviewQuestionView(IQuestion question, 
            InterviewQuestion answeredQuestion, 
            Dictionary<string, string> answersForTitleSubstitution, 
            bool isParentGroupDisabled, 
            decimal[] rosterVector,
            InterviewStatus interviewStatus);

        InterviewStaticTextView BuildInterviewStaticTextView(IStaticText staticText, 
            InterviewStaticText interviewStaticText,
            Dictionary<string, string> answersForTitleSubstitution,
            InterviewAttachmentViewModel attachment);

        InterviewAttachmentViewModel BuildInterviewAttachmentViewModel(string contentId, string contentType, string contentName);
    }
}