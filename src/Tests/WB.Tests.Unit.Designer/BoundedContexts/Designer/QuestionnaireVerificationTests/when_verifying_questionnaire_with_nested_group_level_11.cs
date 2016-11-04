using System;
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
    class when_verifying_questionnaire_with_nested_group_level_11 : QuestionnaireVerifierTestsContext
    {
        static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        static QuestionnaireVerifier verifier;
        static QuestionnaireDocument questionnaire;
        static readonly Guid rosterGroupId = Guid.Parse("10000000000000000000000000000000");

        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocument(
                new Group
                {
                    PublicKey = Guid.NewGuid(),
                    IsRoster = false,
                    VariableName = "a",
                    Children = new List<IComposite>
                    {
                        new TextListQuestion
                        {
                            PublicKey = Guid.NewGuid(),
                            StataExportCaption = "var1",
                            QuestionType = QuestionType.TextList
                        },
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
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(questionnaire);

        It should_return_1_message = () =>
            verificationMessages.Count().ShouldEqual(1);

        It should_return_message_reference_with_id_of_rosterGroupId = () =>
            verificationMessages.Single().References.First().Id.ShouldEqual(rosterGroupId);

        It should_return_message_reference_with_type_Group = () =>
            verificationMessages.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Group);

        It should_return_first_error_with_code__WB0055 = () =>
            verificationMessages.Single().Code.ShouldEqual("WB0101");
    }
}