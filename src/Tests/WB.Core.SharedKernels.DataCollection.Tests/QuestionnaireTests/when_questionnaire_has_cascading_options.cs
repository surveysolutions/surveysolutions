using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using It = Machine.Specifications.It;


namespace WB.Core.SharedKernels.DataCollection.Tests.QuestionnaireTests
{
    internal class when_questionnaire_has_cascading_options : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            parentSingleOptionQuestionId = Guid.Parse("9E96D4AB-DF91-4FC9-9585-23FA270B25D7");
            childCascadedComboboxId = Guid.Parse("C6CC807A-3E81-406C-A110-1044AE3FD89B");
            grandChildCascadedComboboxId = Guid.Parse("4C603B8A-3237-4915-96FA-8D1568C679E2");
            questionnaire = CreateQuestionnaire(Guid.NewGuid(), CreateQuestionnaireDocumentWithOneChapter(new MultyOptionsQuestion
            {
                PublicKey = parentSingleOptionQuestionId,
                QuestionType = QuestionType.SingleOption
            },
               new SingleQuestion
               {
                   PublicKey = childCascadedComboboxId,
                   QuestionType = QuestionType.SingleOption,
                   IsCascadingCombobox = true,
                   CascadeFromQuestionId = parentSingleOptionQuestionId
               }, new SingleQuestion
               {
                   PublicKey = grandChildCascadedComboboxId,
                   QuestionType = QuestionType.SingleOption,
                   IsCascadingCombobox = true,
                   CascadeFromQuestionId = childCascadedComboboxId
               }));
        };

        Because of = () => foundQuestions = questionnaire.GetQuestionnaire().GetCascadingQuestionsThatDependUponQuestion(parentSingleOptionQuestionId);

        It should_find_expected_dependant_questions = () => foundQuestions.ShouldContainOnly(childCascadedComboboxId, grandChildCascadedComboboxId);

        static Guid parentSingleOptionQuestionId;
        static Guid childCascadedComboboxId;
        static Guid grandChildCascadedComboboxId;
        static Questionnaire questionnaire;
        static IEnumerable<Guid> foundQuestions;
    }
}

