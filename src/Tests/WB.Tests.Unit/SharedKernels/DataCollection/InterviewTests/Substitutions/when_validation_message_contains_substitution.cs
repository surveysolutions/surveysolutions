using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Spec;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.QuestionnaireEntities;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Substitutions
{
    internal class when_validation_message_contains_substitution : InterviewTestsContext
    {
        private Establish context = () =>
        {
            rosterSizeQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            staticTextId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            questionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            var substitutedVariableName = "subst";
            QuestionnaireDocument questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(id: rosterSizeQuestionId, variable: $"{substitutedVariableName}"),

                Create.Entity.StaticText(publicKey: staticTextId,
                    validationConditions: new List<ValidationCondition> {
                        Create.Entity.ValidationCondition(message: $"error %{substitutedVariableName}%")
                    }),

                Create.Entity.Roster(rosterSizeSourceType: RosterSizeSourceType.Question,
                    rosterSizeQuestionId: rosterSizeQuestionId,
                    title: "Roster",
                    children: new List<IComposite> {
                        Create.Entity.Question(
                            questionId: questionId,
                            validationConditions: new List<ValidationCondition> {
                                Create.Entity.ValidationCondition(message: $"error %{substitutedVariableName}%")
                            })
                    }));

            Guid questionnaireId = Guid.NewGuid();
            var interviewExpressionStateMock = new Mock<ILatestInterviewExpressionState>();
            interviewExpressionStateMock.Setup(x => x.Clone()).Returns(interviewExpressionStateMock.Object);

            var validityChangesQueue = new Queue<ValidityChanges>();
            validityChangesQueue.Enqueue(new ValidityChanges(null, null));
            validityChangesQueue.Enqueue(new ValidityChanges(
                null, 
                null,
                new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>
                {
                    {
                        Create.Entity.Identity(questionId, Create.Entity.RosterVector(0)), new[] {new FailedValidationCondition(0)}.ToReadOnlyCollection()
                    },
                    {
                        Create.Entity.Identity(questionId, Create.Entity.RosterVector(1)), new[] {new FailedValidationCondition(0)}.ToReadOnlyCollection()
                    }
                }, 
                null,
                new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>
                {
                    {
                        Create.Entity.Identity(staticTextId, RosterVector.Empty), new[] {new FailedValidationCondition(0)}.ToReadOnlyCollection()
                    }
                }));

            interviewExpressionStateMock.Setup(x => x.GetStructuralChanges()).Returns(new StructuralChanges());
            interviewExpressionStateMock.Setup(x => x.ProcessValidationExpressions()).Returns(validityChangesQueue.Dequeue);

            var interviewExpressionStatePrototypeProvider = Mock.Of<IInterviewExpressionStatePrototypeProvider>(_
                => _.GetExpressionState(questionnaireId, Moq.It.IsAny<long>()) == interviewExpressionStateMock.Object);


            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                new PlainQuestionnaire(questionnaire, 1));

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository,
                expressionProcessorStatePrototypeProvider: interviewExpressionStatePrototypeProvider);

            events = new EventContext();
        };

        Because of = () =>
            interview.AnswerNumericIntegerQuestion(Guid.NewGuid(), rosterSizeQuestionId, Empty.RosterVector, DateTime.Now, 2);

        It should_raise_title_changed_event_for_1_static_text = () =>
           events.GetEvent<SubstitutionTitlesChanged>().StaticTexts.Length.ShouldEqual(1);

        It should_raise_title_changed_event_for_specified_static_texts = () =>
           events.GetEvent<SubstitutionTitlesChanged>().StaticTexts.ShouldContainOnly(Create.Entity.Identity(staticTextId, RosterVector.Empty));

        It should_raise_title_changed_event_for_2_questions = () =>
           events.GetEvent<SubstitutionTitlesChanged>().Questions.Length.ShouldEqual(2);

        It should_raise_title_changed_event_for_specified_questions = () =>
           events.GetEvent<SubstitutionTitlesChanged>().Questions.ShouldContainOnly(
               Create.Entity.Identity(questionId, Create.Entity.RosterVector(0)),
               Create.Entity.Identity(questionId, Create.Entity.RosterVector(1)));


        static EventContext events;
        static Interview interview;
        static Guid rosterSizeQuestionId;
        static Guid staticTextId;
        static Guid questionId;
    }
}