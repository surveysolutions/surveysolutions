using System;
using System.Collections.Generic;
using System.Linq;
using FFImageLoading.Mock;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Enumerator.Native.Questionnaire.Impl;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewSummaryEventHandlerFunctionalTests
{
    internal class InterviewSummaryDenormalizerTests : InterviewSummaryDenormalizerTestsContext
    {
        [Test]
        public void when_interview_status_changed_to_completed()
        {
            //arrange
            var viewModel = new InterviewSummary();
            var denormalizer = CreateDenormalizer();

            //act
            var updatedModel = denormalizer.Update(viewModel, Create.PublishedEvent.InterviewStatusChanged(InterviewStatus.Completed));

            //assert
            updatedModel.Status.Should().Be(InterviewStatus.Completed);
            updatedModel.WasCompleted.Should().BeTrue();
        }

        [Test]
        public void when_interview_was_completed_and_then_reassigned_to_supervisor()
        {
            //arrange
            var viewModel = new InterviewSummary {WasCompleted = true};
            var denormalizer = CreateDenormalizer();

            //act
            var updatedModel = denormalizer.Update(viewModel, Create.PublishedEvent.InterviewStatusChanged(InterviewStatus.SupervisorAssigned));

            //assert
            updatedModel.Status.Should().Be(InterviewStatus.SupervisorAssigned);
            updatedModel.WasCompleted.Should().BeTrue();
        }

        [Test]
        public void when_answer_on_cascading_question_then_should_correct_resolve_options_with_parent_answer()
        {
            //arrange
            var document = Create.Entity.QuestionnaireDocumentWithCover(children: new IComposite[]
            {
                Create.Entity.Group(children: new IComposite[]
                {
                    Create.Entity.SingleOptionQuestion(Id.g1, isPrefilled: true, answers: new List<Answer>()
                    {
                        new Answer() { AnswerCode = 1, AnswerText = "parent 1" },
                        new Answer() { AnswerCode = 2, AnswerText = "parent 2" },
                    })
                }),
                Create.Entity.Group(children: new IComposite[]
                {
                    Create.Entity.SingleOptionQuestion(Id.g2, isPrefilled: true, cascadeFromQuestionId: Id.g1,
                        answers: new List<Answer>()
                        {
                            new Answer() { AnswerCode = 1, ParentCode = 1, AnswerText = "kid 1 parent 1" },
                            new Answer() { AnswerCode = 2, ParentCode = 1, AnswerText = "kid 2 parent 1" },
                            new Answer() { AnswerCode = 1, ParentCode = 2, AnswerText = "kid 1 parent 2" },
                            new Answer() { AnswerCode = 2, ParentCode = 2, AnswerText = "kid 2 parent 2" },
                        })
                })
            });
            document.EntitiesIdMap = new Dictionary<Guid, int>
            {
                [Id.g1] = 1,
                [Id.g2] = 2,
            };
            var questionOptionsRepository = new QuestionnaireQuestionOptionsRepository();
            var questionnaire = Create.Entity.PlainQuestionnaire(document, 1, questionOptionsRepository: questionOptionsRepository);
            var viewModel = new InterviewSummary
            {
                IdentifyEntitiesValues =
                {
                    new IdentifyEntityValue() { AnswerCode = null, Identifying = true, Id = 1, Entity = new QuestionnaireCompositeItem() { Id = 1 }},
                    new IdentifyEntityValue() { AnswerCode = null, Identifying = true, Id = 2, Entity = new QuestionnaireCompositeItem() { Id = 2 } },
                }
            };
            var denormalizer = CreateDenormalizer(questionnaire: questionnaire);

            //act
            var updatedModel = denormalizer.Update(viewModel, Create.PublishedEvent.SingleOptionQuestionAnswered(Id.g1, Id.g1, 1));
            updatedModel = denormalizer.Update(updatedModel, Create.PublishedEvent.SingleOptionQuestionAnswered(Id.g1, Id.g2, 2));

            //assert
            var entitiesValues = updatedModel.IdentifyEntitiesValues;
            Assert.That(entitiesValues.First().AnswerCode, Is.EqualTo(1));
            Assert.That(entitiesValues.First().Value, Is.EqualTo("parent 1"));
            Assert.That(entitiesValues.Second().AnswerCode, Is.EqualTo(2));
            Assert.That(entitiesValues.Second().Value, Is.EqualTo("kid 2 parent 1"));
        }
    }
}
