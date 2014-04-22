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
    class when_verifying_questionnaire_that_have_linked_multiquestion_with_max_allowed_answers : QuestionnaireVerifierTestsContext
    {

        private Establish context = () =>
        {
            multyOptionsQuestionId = Guid.Parse("10000000000000000000000000000000");
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
                                QuestionType = QuestionType.Text,
                                PublicKey = linkedQuestionId
                            }
                        }
                    },
                    new MultyOptionsQuestion()
                    {
                        PublicKey = multyOptionsQuestionId,
                        Answers = new List<Answer>() { new Answer(), new Answer() },
                        MaxAllowedAnswers = 3,
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

        private static Guid multyOptionsQuestionId;
    }
}
