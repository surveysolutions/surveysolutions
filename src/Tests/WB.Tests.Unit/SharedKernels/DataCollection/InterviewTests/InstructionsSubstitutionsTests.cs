using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class InstructionsSubstitutionsTests : InterviewTestsContext
    {
        [Test]
        public void when_answering_question_used_in_instructions_substitutions() {
            Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");

            Guid substitutedQuestionId = Guid.Parse("88888888888888888888888888888888");
            Guid questionA = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            QuestionnaireDocument questionnaire = CreateQuestionnaireDocumentWithOneChapter(new Group("top level fixed group")
            {
                Children = new List<IComposite>
                {
                    Create.Entity.TextQuestion(questionId: substitutedQuestionId, variable: "subst"),

                    Create.Entity.TextQuestion(questionId: questionA, instruction: "question A %subst%"),
                    
                }.ToReadOnlyCollection()
            });

            IQuestionnaireStorage questionnaireRepository = 
                CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, Create.Entity.PlainQuestionnaire(questionnaire, 1));

            var interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            var eventContext = new EventContext();

            interview.AnswerTextQuestion(Guid.NewGuid(), substitutedQuestionId, Empty.RosterVector, DateTime.Now, "answer");

            eventContext.ShouldContainEvent<SubstitutionTitlesChanged>(x => x.Questions.Length == 1);
            eventContext.ShouldContainEvent<SubstitutionTitlesChanged>(x => x.Questions[0].Id == questionA);
        }
    }
}

