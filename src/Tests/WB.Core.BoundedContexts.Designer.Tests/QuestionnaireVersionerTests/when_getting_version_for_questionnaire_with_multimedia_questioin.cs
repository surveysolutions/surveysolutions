﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireVersionerTests
{
    internal class when_getting_version_for_questionnaire_with_cascading_questioin : QuestionnaireVersionerTestContext
    {
        Establish context = () =>
        {
            questionnaire =
                CreateQuestionnaireDocumentWithOneChapter(new Group()
                {
                    PublicKey = Guid.Parse("22222222222222222222222222222222"),
                    Children = new List<IComposite>()
                    {
                        new SingleQuestion
                        {
                            PublicKey = cascadingParentId
                        },
                        new SingleQuestion
                        {
                            PublicKey = cascadingId,
                            CascadeFromQuestionId = cascadingParentId
                        }
                    }
                });
            versioner = CreateQuestionnaireVersioner();
        };

        Because of = () =>
            version = versioner.GetVersion(questionnaire);

        It should_set_Major_property_to_4 = () =>
            version.Major.ShouldEqual(4);

        It should_set_Minor_property_to_0 = () =>
            version.Minor.ShouldEqual(0);

        It should_set_Patch_property_to_0 = () =>
            version.Patch.ShouldEqual(0);

        private static QuestionnaireVersion version;
        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireVersioner versioner;
        private static readonly Guid cascadingId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid cascadingParentId = Guid.Parse("33333333333333333333333333333333");
    }

    internal class when_getting_version_for_questionnaire_with_multimedia_questioin : QuestionnaireVersionerTestContext
    {
        Establish context = () =>
        {
            questionnaire =
                CreateQuestionnaireDocumentWithOneChapter(new Group()
                {
                    PublicKey = Guid.Parse("22222222222222222222222222222222"),
                    Children = new List<IComposite>()
                    {
                        new MultimediaQuestion()
                        {
                            PublicKey = Guid.Parse("11111111111111111111111111111111")
                        }
                    }
                });
            versioner = CreateQuestionnaireVersioner();
        };

        Because of = () =>
            version = versioner.GetVersion(questionnaire);

        It should_set_Major_property_to_4 = () =>
            version.Major.ShouldEqual(4);

        It should_set_Minor_property_to_0 = () =>
            version.Minor.ShouldEqual(0);

        It should_set_Patch_property_to_0 = () =>
            version.Patch.ShouldEqual(0);

        private static QuestionnaireVersion version;
        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireVersioner versioner;
    }
}
