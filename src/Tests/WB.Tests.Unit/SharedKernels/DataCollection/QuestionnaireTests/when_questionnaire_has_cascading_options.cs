﻿using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using It = Machine.Specifications.It;


namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireTests
{
    internal class when_questionnaire_has_cascading_options : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            parentSingleOptionQuestionId = Guid.Parse("9E96D4AB-DF91-4FC9-9585-23FA270B25D7");
            childCascadedComboboxId = Guid.Parse("C6CC807A-3E81-406C-A110-1044AE3FD89B");
            grandChildCascadedComboboxId = Guid.Parse("4C603B8A-3237-4915-96FA-8D1568C679E2");
            questionnaire =  CreateQuestionnaireDocumentWithOneChapter(new MultyOptionsQuestion
            {
                PublicKey = parentSingleOptionQuestionId,
                QuestionType = QuestionType.SingleOption
            },
               new SingleQuestion
               {
                   PublicKey = childCascadedComboboxId,
                   QuestionType = QuestionType.SingleOption,
                   CascadeFromQuestionId = parentSingleOptionQuestionId
               },
               new SingleQuestion
               {
                   PublicKey = grandChildCascadedComboboxId,
                   QuestionType = QuestionType.SingleOption,
                   CascadeFromQuestionId = childCascadedComboboxId
               });
        };

        Because of = () => foundQuestions = new PlainQuestionnaire(questionnaire, 1).GetCascadingQuestionsThatDependUponQuestion(parentSingleOptionQuestionId);

        It should_find_expected_dependant_questions = () => foundQuestions.ShouldContainOnly(childCascadedComboboxId, grandChildCascadedComboboxId);

        static Guid parentSingleOptionQuestionId;
        static Guid childCascadedComboboxId;
        static Guid grandChildCascadedComboboxId;
        static QuestionnaireDocument questionnaire;
        static IEnumerable<Guid> foundQuestions;
    }
}

