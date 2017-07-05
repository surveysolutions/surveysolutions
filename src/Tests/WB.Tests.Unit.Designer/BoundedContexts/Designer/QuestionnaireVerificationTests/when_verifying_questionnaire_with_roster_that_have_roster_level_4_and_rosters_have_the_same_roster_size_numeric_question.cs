﻿using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_roster_that_have_roster_level_4_and_rosters_have_the_same_roster_size_numeric_question : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            var rosterSizeQuestionId = Guid.Parse("20000000000000000000000000000000");
            questionnaire = CreateQuestionnaireDocument(new IComposite[]
            {
                new NumericQuestion() {PublicKey = rosterSizeQuestionId, IsInteger = true, StataExportCaption = "var"},
                new Group()
                {
                    PublicKey = Guid.NewGuid(),
                    IsRoster = true,
                    VariableName = "a",
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IComposite>()
                    {
                        new Group()
                        {
                            PublicKey = Guid.NewGuid(),
                            IsRoster = true,
                            VariableName = "b",
                            RosterSizeQuestionId = rosterSizeQuestionId,
                            Children = new List<IComposite>()
                            {
                                new Group()
                                {
                                    PublicKey = Guid.NewGuid(),
                                    IsRoster = true,
                                    VariableName = "c",
                                    RosterSizeQuestionId = rosterSizeQuestionId
                                }
                            }.ToReadOnlyCollection()
                        }
                    }.ToReadOnlyCollection()
                }
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_first_error_with_code__WB0261 = () =>
            verificationMessages.ShouldContainError("WB0261");

        It should_return_first_error_with_code__WB0262 = () =>
            verificationMessages.ShouldContainError("WB0262");

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
    }
}
