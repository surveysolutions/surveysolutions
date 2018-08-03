using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    class when_verifying_questionnaire_with_nested_group_level_11 : QuestionnaireVerifierTestsContext
    {
        static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        static QuestionnaireVerifier verifier;
        static QuestionnaireDocument questionnaire;
        static readonly Guid rosterGroupId = Guid.Parse("10000000000000000000000000000000");

        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaireDocument(
                new Group
                {
                    PublicKey = Guid.NewGuid(),
                    IsRoster = false,
                    VariableName = "a",
                    Children = new List<IComposite>
                    {
                        Create.TextListQuestion
                        (
                            questionId: Guid.NewGuid(),
                            variable: "var1"
                        ),
                        new Group
                        {
                            PublicKey = Guid.NewGuid(),
                            IsRoster = false,
                            VariableName = "a",
                            Children = new List<IComposite>
                            {
                                new Group
                                {
                                    PublicKey = Guid.NewGuid(),
                                    IsRoster = false,
                                    VariableName = "a",
                                    Children = new List<IComposite>
                                    {
                                        new Group
                                        {
                                            PublicKey = Guid.NewGuid(),
                                            IsRoster = false,
                                            VariableName = "a",
                                            Children = new List<IComposite>
                                            {
                                                new Group
                                                {
                                                    PublicKey = Guid.NewGuid(),
                                                    IsRoster = false,
                                                    VariableName = "a",
                                                    Children = new List<IComposite>
                                                    {
                                                        new Group
                                                        {
                                                            PublicKey = Guid.NewGuid(),
                                                            IsRoster = false,
                                                            VariableName = "a",
                                                            Children = new List<IComposite>
                                                            {
                                                                new Group
                                                                {
                                                                    PublicKey = Guid.NewGuid(),
                                                                    IsRoster = false,
                                                                    VariableName = "a",
                                                                    Children = new List<IComposite>
                                                                    {
                                                                        new Group
                                                                        {
                                                                            PublicKey = Guid.NewGuid(),
                                                                            IsRoster = false,
                                                                            VariableName = "a",
                                                                            Children = new List<IComposite>
                                                                            {
                                                                                new Group
                                                                                {
                                                                                    PublicKey = Guid.NewGuid(),
                                                                                    IsRoster = false,
                                                                                    VariableName = "a",
                                                                                    Children = new List<IComposite>
                                                                                    {
                                                                                        new Group
                                                                                        {
                                                                                            PublicKey = Guid.NewGuid(),
                                                                                            IsRoster = false,
                                                                                            VariableName = "a",
                                                                                            Children =
                                                                                                new List<IComposite>
                                                                                                {
                                                                                                    new Group
                                                                                                    {
                                                                                                        PublicKey =
                                                                                                            rosterGroupId,
                                                                                                        IsRoster = false,
                                                                                                        VariableName =
                                                                                                            "a"
                                                                                                    }
                                                                                                }.ToReadOnlyCollection()
                                                                                        }
                                                                                    }.ToReadOnlyCollection()
                                                                                }
                                                                            }.ToReadOnlyCollection()
                                                                        }
                                                                    }.ToReadOnlyCollection()
                                                                }
                                                            }.ToReadOnlyCollection()
                                                        }
                                                    }.ToReadOnlyCollection()
                                                }
                                            }.ToReadOnlyCollection()
                                        }
                                    }.ToReadOnlyCollection()
                                }
                            }.ToReadOnlyCollection()
                        }
                    }.ToReadOnlyCollection()
                });


            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_message_reference_with_id_of_rosterGroupId () =>
            verificationMessages.GetError("WB0101").References.First().Id.Should().Be(rosterGroupId);

        [NUnit.Framework.Test] public void should_return_message_reference_with_type_Group () =>
            verificationMessages.GetError("WB0101").References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Group);

        [NUnit.Framework.Test] public void should_return_first_error_with_code__WB0055 () =>
            verificationMessages.ShouldContainError("WB0101");
    }
}