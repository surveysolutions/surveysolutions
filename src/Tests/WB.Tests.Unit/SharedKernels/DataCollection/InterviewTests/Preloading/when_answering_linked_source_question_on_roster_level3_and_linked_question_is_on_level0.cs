using System;
using FluentAssertions;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Preloading;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;
using WB.Tests.Abc.TestFactories;
using WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Preloading
{
    internal class when_creating_interview_with_preloaded_data_with_nested_rosters : StatefulInterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
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

            interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaireDocument, shouldBeInitialized: false);

            PreloadedLevelDto[] preloadedLevelDtos = new[]
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
            };

            command = Create.Command.CreateInterview(Guid.NewGuid(),
                Guid.NewGuid(), Guid.NewGuid(), 1, new PreloadedDataDto(preloadedLevelDtos).Answers, Guid.NewGuid());
            BecauseOf();
        }

        public void BecauseOf() =>
            interview.CreateInterview(command);

        [NUnit.Framework.Test] public void should_set_answers_for_text_question_on_the_3rd_level () 
        {
            interview.GetTextQuestion(Create.Entity.Identity(textQuestionId, Create.Entity.RosterVector(0, 1, 5)))
                .GetAnswer().Value.Should().Be("aaa");

            interview.GetTextQuestion(Create.Entity.Identity(textQuestionId, Create.Entity.RosterVector(0, 1, 9)))
                .GetAnswer().Value.Should().Be("bbb");

            interview.GetTextQuestion(Create.Entity.Identity(textQuestionId, Create.Entity.RosterVector(0, 2, 3)))
                .GetAnswer().Value.Should().Be("ccc");

            interview.GetTextQuestion(Create.Entity.Identity(textQuestionId, Create.Entity.RosterVector(0, 2, 4)))
                .GetAnswer().Value.Should().Be("ddd");
        }

        static StatefulInterview interview;

        static readonly Guid roster1Id = Guid.Parse("11111111111111111111111111111111");
        static readonly Guid roster2Id = Guid.Parse("22222222222222222222222222222222");
        static readonly Guid roster3Id = Guid.Parse("33333333333333333333333333333333");

        static readonly Guid numericRosterSizeQuestionId = Guid.Parse("44444444444444444444444444444444");
        static readonly Guid multiRosterSizeQuestionId = Guid.Parse("55555555555555555555555555555555");
        static readonly Guid listRosterSizeQuestionId = Guid.Parse("66666666666666666666666666666666");
        static readonly Guid textQuestionId = Guid.Parse("77777777777777777777777777777777");

        static CreateInterview command;
    }
}
