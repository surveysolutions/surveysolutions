using System;
using System.Collections.Generic;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Preloading;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Enumerator.Native.Questionnaire.Impl;
using WB.Tests.Abc;
using WB.Tests.Abc.TestFactories;
using WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Preloading
{
    internal class when_preloding_cascading_in_revese_order : StatefulInterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.SingleOptionQuestion(questionId: singleOptionLevel1, 
                    variable: "singleOptionLevel1",
                    answers: new List<Answer>()
                    {
                        Create.Entity.Answer("EnValue1", 1),
                        Create.Entity.Answer("EnValue2", 2),
                    }),
                Create.Entity.SingleOptionQuestion(questionId: singleOptionLevel2,
                    cascadeFromQuestionId: singleOptionLevel1,
                    variable: "singleOptionLevel2",
                    answers: new List<Answer>()
                    {
                        Create.Entity.Answer("EnValue1", 1, 1),
                        Create.Entity.Answer("EnValue2", 2, 2),
                    }),
                Create.Entity.SingleOptionQuestion(questionId: singleOptionLevel3,
                    cascadeFromQuestionId: singleOptionLevel2,
                    variable: "singleOptionLevel3",
                    answers: new List<Answer>()
                    {
                        Create.Entity.Answer("EnValue1", 1, 1),
                        Create.Entity.Answer("EnValue2", 2, 2),
                    }),
                Create.Entity.SingleOptionQuestion(questionId: singleOptionLevel4,
                    cascadeFromQuestionId: singleOptionLevel3,
                    variable: "singleOptionLevel4",
                    answers: new List<Answer>()
                    {
                        Create.Entity.Answer("EnValue1", 1, 1),
                        Create.Entity.Answer("EnValue2", 2, 2),
                    }),
                Create.Entity.SingleOptionQuestion(questionId: singleOption,
                    variable: "singleOption",
                    answers: new List<Answer>()
                    {
                        Create.Entity.Answer("EnValue1", 1),
                        Create.Entity.Answer("EnValue2", 2),
                    })
            });

            interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaireDocument, 
                optionsRepository: new QuestionnaireQuestionOptionsRepository(),
                shouldBeInitialized: false);

            PreloadedLevelDto[] preloadedLevelDtos = new[]
            {
                Create.Entity.PreloadedLevelDto(Create.Entity.RosterVector(),
                    new PreloadedAnswer(singleOptionLevel4, Create.Entity.SingleOptionAnswer(2))),
                
                Create.Entity.PreloadedLevelDto(Create.Entity.RosterVector(),
                    new PreloadedAnswer(singleOption, Create.Entity.SingleOptionAnswer(2))),
                
                Create.Entity.PreloadedLevelDto(Create.Entity.RosterVector(),
                    new PreloadedAnswer(singleOptionLevel3, Create.Entity.SingleOptionAnswer(2))),
                
                Create.Entity.PreloadedLevelDto(Create.Entity.RosterVector(),
                    new PreloadedAnswer(singleOptionLevel1, Create.Entity.SingleOptionAnswer(2))),
                
                Create.Entity.PreloadedLevelDto(Create.Entity.RosterVector(),
                    new PreloadedAnswer(singleOptionLevel2, Create.Entity.SingleOptionAnswer(2))),
                
            };

            command = Create.Command.CreateInterview(Guid.NewGuid(),
                Guid.NewGuid(), Guid.NewGuid(), 1, 
                new PreloadedDataDto(preloadedLevelDtos).GetAnswers(), Guid.NewGuid());
            BecauseOf();
        }

        public void BecauseOf() =>
            interview.CreateInterview(command);

        [NUnit.Framework.Test] public void should_set_answers_for_text_question_on_the_3rd_level () 
        {
            interview.GetSingleOptionQuestion(Create.Entity.Identity(singleOptionLevel1, Create.Entity.RosterVector()))
                .GetAnswer().SelectedValue.Should().Be(2);

            interview.GetSingleOptionQuestion(Create.Entity.Identity(singleOptionLevel2, Create.Entity.RosterVector()))
                .GetAnswer().SelectedValue.Should().Be(2);
            
            interview.GetSingleOptionQuestion(Create.Entity.Identity(singleOptionLevel3, Create.Entity.RosterVector()))
                .GetAnswer().SelectedValue.Should().Be(2);

            interview.GetSingleOptionQuestion(Create.Entity.Identity(singleOptionLevel4, Create.Entity.RosterVector()))
                .GetAnswer().SelectedValue.Should().Be(2);

            interview.GetSingleOptionQuestion(Create.Entity.Identity(singleOption, Create.Entity.RosterVector()))
                .GetAnswer().SelectedValue.Should().Be(2);
        }

        static StatefulInterview interview;

        static readonly Guid singleOptionLevel1 = Guid.Parse("11111111111111111111111111111111");
        static readonly Guid singleOptionLevel2 = Guid.Parse("22222222222222222222222222222222");
        static readonly Guid singleOptionLevel3 = Guid.Parse("33333333333333333333333333333333");
        static readonly Guid singleOptionLevel4 = Guid.Parse("44444444444444444444444444444444");
        static readonly Guid singleOption = Guid.Parse("55555555555555555555555555555555");
        
        static CreateInterview command;
    }
}
