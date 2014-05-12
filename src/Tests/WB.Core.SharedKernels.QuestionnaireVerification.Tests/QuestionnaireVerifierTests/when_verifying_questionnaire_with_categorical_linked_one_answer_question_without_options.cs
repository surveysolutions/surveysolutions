﻿using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.QuestionnaireVerification.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Tests.QuestionnaireVerifierTests
{
    class when_verifying_questionnaire_with_categorical_linked_one_answer_question_without_options : QuestionnaireVerifierTestsContext
    {

        private Establish context = () =>
        {
            var questionId = Guid.Parse("10000000000000000000000000000000");
            var linkedQuestionId = Guid.Parse("20000000000000000000000000000000");
            var rosterSizeQuestion = Guid.Parse("30000000000000000000000000000000");
            var rosterGroup = Guid.Parse("40000000000000000000000000000000");
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(new Group("Group")
            {
                Children = new List<IComposite>()
                {
                    new NumericQuestion("Roster Size Question")
                    {
                        PublicKey = rosterSizeQuestion,
                        StataExportCaption = "var",
                        IsInteger = true,
                        MaxValue = 5
                    },
                    new Group("Roster Group")
                    {
                        PublicKey = rosterGroup,
                        IsRoster = true,
                        RosterSizeSource = RosterSizeSourceType.Question,
                        RosterSizeQuestionId = rosterSizeQuestion,
                        Children = new List<IComposite>()
                        {
                            new TextQuestion("TextQuestion")
                            {
                                StataExportCaption = "var",
                                QuestionType = QuestionType.Text,
                                PublicKey = linkedQuestionId
                            }
                        }
                    },
                    new SingleQuestion()
                    {
                        StataExportCaption = "var",
                        PublicKey = questionId,
                        LinkedToQuestionId = linkedQuestionId
                    }
                }
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () => 
            resultErrors = verifier.Verify(questionnaire);

        It should_return_no_errors = () =>
             resultErrors.ShouldBeEmpty();

        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
    }
}
