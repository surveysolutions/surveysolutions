using System;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests;
using WB.Tests.Unit.TestFactories;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Preloading
{
    internal class when_creating_interview_with_preloaded_data_with_nested_rosters : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.NumericIntegerQuestion(id: numericRosterSizeQuestionId),
                Create.Entity.Roster(rosterId: roster1Id, rosterSizeQuestionId: numericRosterSizeQuestionId, children: new IComposite[]
                {
                    Create.Entity.MultipleOptionsQuestion(questionId: multiRosterSizeQuestionId, answers: new [] {1, 2}),
                    Create.Entity.Roster(rosterId: roster2Id, rosterSizeQuestionId: multiRosterSizeQuestionId, children: new IComposite[]
                    {
                        Create.Entity.TextListQuestion(questionId: listRosterSizeQuestionId),
                        Create.Entity.Roster(roster3Id, rosterSizeQuestionId: listRosterSizeQuestionId, children: new IComposite[]
                        {
                            Create.Entity.TextQuestion(questionId: textQuestionId)
                        })
                    })
                })
            });
            var plainQuestionnaire = new PlainQuestionnaire(questionnaireDocument, 0);

            interview = Create.AggregateRoot.StatefulInterview(questionnaire: plainQuestionnaire, shouldBeInitialized: false);
        };

        Because of = () =>
            interview.CreateInterviewWithPreloadedData(Create.Command.CreateInterviewWithPreloadedData(new []
            {
                Create.Entity.PreloadedLevelDto(Create.Entity.RosterVector(0, 2, 3),
                    new PreloadedAnswer(textQuestionId, Create.Entity.TextQuestionAnswer("ccc"))),
                Create.Entity.PreloadedLevelDto(Create.Entity.RosterVector(0, 2, 4),
                    new PreloadedAnswer(textQuestionId, Create.Entity.TextQuestionAnswer("ddd"))),
                 Create.Entity.PreloadedLevelDto(Create.Entity.RosterVector(0, 2),
                    new PreloadedAnswer(listRosterSizeQuestionId, Create.Entity.ListAnswer(3, 4))),
                Create.Entity.PreloadedLevelDto(Create.Entity.RosterVector(0), 
                    new PreloadedAnswer(multiRosterSizeQuestionId, Create.Entity.MultiOptionAnswer(1, 2))),
                Create.Entity.PreloadedLevelDto(Create.Entity.RosterVector(0, 1),
                    new PreloadedAnswer(listRosterSizeQuestionId, Create.Entity.ListAnswer(5, 9))),
                Create.Entity.PreloadedLevelDto(Create.Entity.RosterVector(0, 1, 5),
                    new PreloadedAnswer(textQuestionId, Create.Entity.TextQuestionAnswer("aaa"))),
                Create.Entity.PreloadedLevelDto(Create.Entity.RosterVector(0, 1, 9),
                    new PreloadedAnswer(textQuestionId, Create.Entity.TextQuestionAnswer("bbb"))),
                Create.Entity.PreloadedLevelDto(RosterVector.Empty, new PreloadedAnswer(numericRosterSizeQuestionId,
                    Create.Entity.NumericIntegerAnswer(1))),
            }));

        It should_set_answers_for_text_question_on_the_3rd_level = () =>
        {
            interview.GetTextQuestion(Create.Entity.Identity(textQuestionId, Create.Entity.RosterVector(0, 1, 5)))
                .GetAnswer().Value.ShouldEqual("aaa");

            interview.GetTextQuestion(Create.Entity.Identity(textQuestionId, Create.Entity.RosterVector(0, 1, 9)))
                .GetAnswer().Value.ShouldEqual("bbb");

            interview.GetTextQuestion(Create.Entity.Identity(textQuestionId, Create.Entity.RosterVector(0, 2, 3)))
                .GetAnswer().Value.ShouldEqual("ccc");

            interview.GetTextQuestion(Create.Entity.Identity(textQuestionId, Create.Entity.RosterVector(0, 2, 4)))
                .GetAnswer().Value.ShouldEqual("ddd");
        };

        static StatefulInterview interview;

        static readonly Guid roster1Id = Guid.Parse("11111111111111111111111111111111");
        static readonly Guid roster2Id = Guid.Parse("22222222222222222222222222222222");
        static readonly Guid roster3Id = Guid.Parse("33333333333333333333333333333333");

        static readonly Guid numericRosterSizeQuestionId = Guid.Parse("44444444444444444444444444444444");
        static readonly Guid multiRosterSizeQuestionId = Guid.Parse("55555555555555555555555555555555");
        static readonly Guid listRosterSizeQuestionId = Guid.Parse("66666666666666666666666666666666");
        static readonly Guid textQuestionId = Guid.Parse("77777777777777777777777777777777");
    }
}