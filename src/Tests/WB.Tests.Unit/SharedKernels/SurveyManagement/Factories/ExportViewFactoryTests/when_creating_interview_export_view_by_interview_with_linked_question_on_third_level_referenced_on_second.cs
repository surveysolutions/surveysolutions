using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewExportedDataEventHandlerTests;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    internal class when_creating_interview_export_view_by_interview_with_linked_question_on_third_level_referenced_on_second : ExportViewFactoryTestsContext
    {
        Establish context = () =>
        {
            linkedQuestionSourceId = Guid.Parse("12222222222222222222222222222222");
            rosterId = Guid.Parse("13333333333333333333333333333333");
            nestedRosterId = Guid.Parse("23333333333333333333333333333333");

            linkedQuestionId = Guid.Parse("10000000000000000000000000000000");

            questionnarie = CreateQuestionnaireDocumentWithOneChapter(
                new Group()
                {
                    PublicKey = rosterId,
                    IsRoster = true,
                    RosterFixedTitles = new string[] { "t1", "t2" },
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    Children = new List<IComposite>
                    {
                        new NumericQuestion()
                        {
                            PublicKey = linkedQuestionSourceId,
                            QuestionType = QuestionType.Numeric,
                            StataExportCaption = "q1"
                        },
                        new Group()
                        {
                            PublicKey = nestedRosterId,
                            IsRoster = true,
                            RosterFixedTitles = new string[] { "n1", "n2" },
                            RosterSizeSource = RosterSizeSourceType.FixedTitles,
                            Children = new List<IComposite>
                            {

                                new SingleQuestion()
                                {
                                    PublicKey = linkedQuestionId,
                                    QuestionType = QuestionType.SingleOption,
                                    LinkedToQuestionId = linkedQuestionSourceId
                                }
                            }
                        }
                    }
                });

            interview = CreateInterviewData();
            var rosterLevel = new InterviewLevel(new ValueVector<Guid>{ rosterId, nestedRosterId }, null, new decimal[] { 0, 0 });
            interview.Levels.Add("0,0", rosterLevel);

            if (!rosterLevel.QuestionsSearchCahche.ContainsKey(linkedQuestionId))
                rosterLevel.QuestionsSearchCahche.Add(linkedQuestionId, new InterviewQuestion(linkedQuestionId));

            var textListQuestion = rosterLevel.QuestionsSearchCahche[linkedQuestionId];
            textListQuestion.Answer = new decimal[] { 0 };
            exportViewFactory = CreateExportViewFactory();
        };

        Because of = () =>
               result = exportViewFactory.CreateInterviewDataExportView(exportViewFactory.CreateQuestionnaireExportStructure(questionnarie,1),
                interview);

        It should_linked_question_have_one_answer = () =>
           GetLevel(result, new[] { rosterId, nestedRosterId }).Records[0].Questions[0].Answers.Length.ShouldEqual(1);

        It should_linked_question_have_first_answer_be_equal_to_0 = () =>
           GetLevel(result, new[] { rosterId, nestedRosterId }).Records[0].Questions[0].Answers[0].ShouldEqual("0");

        private static InterviewDataExportView result;
        private static Guid rosterId;
        private static Guid linkedQuestionId;
        private static Guid linkedQuestionSourceId;
        private static QuestionnaireDocument questionnarie;
        private static Guid nestedRosterId;
        private static InterviewData interview;
        private static ExportViewFactory exportViewFactory;
    }
}
