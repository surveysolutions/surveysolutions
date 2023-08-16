using System;
using System.Collections.Generic;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Tests.Abc;



namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireTests
{
    internal class when_questionnaire_has_cascading_options : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            parentSingleOptionQuestionId = Guid.Parse("9E96D4AB-DF91-4FC9-9585-23FA270B25D7");
            childCascadedComboboxId = Guid.Parse("C6CC807A-3E81-406C-A110-1044AE3FD89B");
            grandChildCascadedComboboxId = Guid.Parse("4C603B8A-3237-4915-96FA-8D1568C679E2");
            questionnaire =  CreateQuestionnaireDocumentWithOneChapter(new MultyOptionsQuestion
            {
                PublicKey = parentSingleOptionQuestionId
            },
               new SingleQuestion
               {
                   PublicKey = childCascadedComboboxId,
                   CascadeFromQuestionId = parentSingleOptionQuestionId
               },
               new SingleQuestion
               {
                   PublicKey = grandChildCascadedComboboxId,
                   CascadeFromQuestionId = childCascadedComboboxId
               });
            BecauseOf();
        }

        public void BecauseOf() => foundQuestions = Create.Entity.PlainQuestionnaire(questionnaire, 1).GetCascadingQuestionsThatDependUponQuestion(parentSingleOptionQuestionId);

        [NUnit.Framework.Test] public void should_find_expected_dependant_questions () => foundQuestions.Should()
            .BeEquivalentTo(new[]{ childCascadedComboboxId, grandChildCascadedComboboxId });

        static Guid parentSingleOptionQuestionId;
        static Guid childCascadedComboboxId;
        static Guid grandChildCascadedComboboxId;
        static QuestionnaireDocument questionnaire;
        static IEnumerable<Guid> foundQuestions;
    }
}

