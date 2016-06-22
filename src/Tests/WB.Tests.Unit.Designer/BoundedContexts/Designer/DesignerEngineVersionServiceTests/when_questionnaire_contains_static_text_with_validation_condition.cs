using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.DesignerEngineVersionServiceTests
{
    internal class when_questionnaire_contains_static_text_with_validation_condition
    {
        Establish context = () =>
        {
            Guid staticTextId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionnaire = Create.QuestionnaireDocument(children: new IComposite[]
                {
                    Create.StaticText(staticTextId: staticTextId, validationConditions: new List<ValidationCondition>
                    {
                        Create.ValidationCondition("expression 1", "message 1"),
                        Create.ValidationCondition("expression 2", "message 2")
                    })
                }
            );
            designerEngineVersionService = Create.DesignerEngineVersionService();
        };

        Because of = () => calculatedVersion = designerEngineVersionService.GetQuestionnaireContentVersion(questionnaire);

        It should_return_version_14 = () => calculatedVersion.ShouldEqual(14);

        static QuestionnaireDocument questionnaire;
        static IDesignerEngineVersionService designerEngineVersionService;
        static int calculatedVersion;
    }
}