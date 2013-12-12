using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Capi.Tests.Views.InterviewViewModel
{
    internal class InterviewViewModelTestContext
    {
        protected static Capi.Views.InterviewDetails.InterviewViewModel CreateInterviewViewModel(QuestionnaireDocument template,
            QuestionnaireRosterStructure rosterStructure, InterviewSynchronizationDto interviewSynchronizationDto)
        {
            return new Capi.Views.InterviewDetails.InterviewViewModel(Guid.NewGuid(), template, rosterStructure,
                interviewSynchronizationDto);
        }

        protected static InterviewSynchronizationDto CreateInterviewSynchronizationDto(AnsweredQuestionSynchronizationDto[] answers,
            Dictionary<InterviewItemId, HashSet<decimal>> propagatedGroupInstanceCounts)
        {
            return new InterviewSynchronizationDto(id: Guid.NewGuid(), status: InterviewStatus.InterviewerAssigned,
                userId: Guid.NewGuid(), questionnaireId: Guid.NewGuid(), questionnaireVersion: 1,
                answers: answers,
                disabledGroups: new HashSet<InterviewItemId>(),
                disabledQuestions: new HashSet<InterviewItemId>(), validAnsweredQuestions: new HashSet<InterviewItemId>(),
                invalidAnsweredQuestions: new HashSet<InterviewItemId>(),
                propagatedGroupInstanceCounts: propagatedGroupInstanceCounts,
                wasCompleted: false);
        }

        protected static QuestionnaireRosterStructure CreateQuestionnaireRosterStructure(QuestionnaireDocument template)
        {
            return new QuestionnaireRosterStructure(template, 1);
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocumentWithOneChapter(params IComposite[] chapterChildren)
        {
            return new QuestionnaireDocument
            {
                Children = new List<IComposite>
                {
                    new Group("Chapter")
                    {
                        PublicKey = Guid.Parse("FFF000AAA111EE2DD2EE111AAA000FFF"),
                        Children = chapterChildren.ToList(),
                    }
                }
            };
        }
    }
}
