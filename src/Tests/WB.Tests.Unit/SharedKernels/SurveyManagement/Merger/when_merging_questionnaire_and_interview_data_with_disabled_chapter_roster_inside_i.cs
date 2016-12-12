using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Merger
{
    class when_merging_questionnaire_and_interview_data_with_disabled_chapter_roster_inside_it_and_question_inside_roster : InterviewDataAndQuestionnaireMergerTestContext
    {
        Establish context = () =>
        {
            var chapterId = Guid.Parse("44444444444444444444444444444444");

            questionnaire = new QuestionnaireDocument
            {
                Children = new List<IComposite>
                {
                    new Group("Chapter")
                    {
                        PublicKey = chapterId,
                        Children =    new List<IComposite>()
                        {
                            new Group(nestedGroupTitle)
                            {
                                PublicKey = nestedGroupId,
                                IsRoster = true,
                                RosterSizeSource = RosterSizeSourceType.FixedTitles,
                                Children =
                                    new List<IComposite>()
                                    {
                                        new NumericQuestion()
                                        {
                                            PublicKey = questionInNestedGroupId,
                                            QuestionType = QuestionType.Numeric,
                                            StataExportCaption = "q2"
                                        }
                                    }.ToReadOnlyCollection()
                            }
                        }.ToReadOnlyCollection()
                    }
                }.ToReadOnlyCollection()
            };

            interview = CreateInterviewData(interviewId);

            interview.Levels["#"].DisabledGroups.Add(chapterId);
            AddInterviewLevel(interview, new ValueVector<Guid> { nestedGroupId }, new decimal[] { 0 }, new Dictionary<Guid, object> { { questionInNestedGroupId, 5 } });
            
            user = Mock.Of<UserDocument>();

            merger = CreateMerger(questionnaire);
        };

        Because of = () =>
            mergeResult = merger.Merge(interview, questionnaire, user.GetUseLight(), null, null);

        It should_question_be_disabled = () =>
            ((InterviewQuestionView)mergeResult.Groups.FirstOrDefault(g => g.Id == nestedGroupId).Entities[0]).IsEnabled.ShouldEqual(false);

        private static InterviewDataAndQuestionnaireMerger merger;
        private static InterviewDetailsView mergeResult;
        private static InterviewData interview;
        private static QuestionnaireDocument questionnaire;
        private static UserDocument user;
        private static Guid nestedGroupId = Guid.Parse("11111111111111111111111111111111");
        private static Guid questionInNestedGroupId = Guid.Parse("55555555555555555555555555555555");
        private static Guid rosterId = Guid.Parse("22222222222222222222222222222222");
        private static Guid interviewId = Guid.Parse("33333333333333333333333333333333");
        private static string nestedGroupTitle = "nested Group";
    }
}
