﻿using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.QuestionnaireVerification.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Tests.QuestionnaireVerifierTests
{
    internal class when_verifying_questionnaire_with_roster_that_have_roster_level_more_than_4_and_rosters_have_the_same_roster_size_question : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            var rosterSizeQuestionId = Guid.Parse("20000000000000000000000000000000");
            questionnaire = CreateQuestionnaireDocument(new IComposite[]
            {
                new NumericQuestion() {PublicKey = rosterSizeQuestionId, IsInteger = true, MaxValue = 5},
                new Group()
                {
                    PublicKey = Guid.NewGuid(),
                    IsRoster = true,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IComposite>()
                    {
                        new Group()
                        {
                            PublicKey = Guid.NewGuid(),
                            IsRoster = true,
                            RosterSizeQuestionId = rosterSizeQuestionId,
                            Children = new List<IComposite>()
                            {
                                new Group()
                                {
                                    PublicKey = Guid.NewGuid(),
                                    IsRoster = true,
                                    RosterSizeQuestionId = rosterSizeQuestionId,
                                    Children = new List<IComposite>()
                                    {
                                        new Group()
                                        {
                                            PublicKey = Guid.NewGuid(),
                                            IsRoster = true,
                                            RosterSizeQuestionId = rosterSizeQuestionId,
                                            Children = new List<IComposite>()
                                            {
                                                new Group()
                                                {
                                                    PublicKey = Guid.NewGuid(),
                                                    IsRoster = true,
                                                    RosterSizeQuestionId = rosterSizeQuestionId,
                                                    Children = new List<IComposite>()
                                                    {
                                                        new Group()
                                                        {
                                                            PublicKey = rosterGroupId,
                                                            IsRoster = true,
                                                            RosterSizeQuestionId = rosterSizeQuestionId,
                                                            Children = new List<IComposite>()
                                                            {

                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_first_error_with_code__WB0055 = () =>
            resultErrors.Single().Code.ShouldEqual("WB0055");

        It should_return_error_reference_with_type_Question = () =>
            resultErrors.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Group);

        It should_return_error_reference_with_id_of_rosterGroupId = () =>
            resultErrors.Single().References.First().Id.ShouldEqual(rosterGroupId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid rosterGroupId = Guid.Parse("10000000000000000000000000000000");
    }
}
