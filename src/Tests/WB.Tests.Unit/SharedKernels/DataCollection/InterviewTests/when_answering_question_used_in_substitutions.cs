using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answering_question_used_in_substitutions : InterviewTestsContext
    {
        Establish context = () =>
        {
            substitutedQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            questionA = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionB = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");

            QuestionnaireDocument questionnaire = CreateQuestionnaireDocumentWithOneChapter(new Group("top level fixed group")
            {
                Children = new List<IComposite>()
                {
                    new TextQuestion
                    {
                        QuestionType = QuestionType.Text,
                        PublicKey = substitutedQuestionId,
                        StataExportCaption = "subst"
                    },
                    new TextQuestion
                    {
                        QuestionType = QuestionType.Text,
                        PublicKey = questionA,
                        QuestionText = "question A %subst%"
                    },
                    new TextQuestion
                    {
                        QuestionType = QuestionType.Text,
                        PublicKey = questionB,
                        QuestionText = "question B %subst%"
                    }
                }
            });

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                new PlainQuestionnaire(questionnaire, 1));

            SetupInstanceToMockedServiceLocator<IQuestionnaireRepository>(questionnaireRepository);

            interview = CreateInterview(questionnaireId: questionnaireId);

            events = new EventContext();
        };

        Because of = () => interview.AnswerTextQuestion(Guid.NewGuid(), substitutedQuestionId, Empty.RosterVector, DateTime.Now, "answer");

        It should_raise_substitution_changed_event = () => events.ShouldContainEvent<SubstitutionTitlesChanged>(x => x.Questions.Length == 2);

        static Guid substitutedQuestionId;
        static Guid questionA;
        static Guid questionB;
        static Interview interview;
        static EventContext events;
    }
}

