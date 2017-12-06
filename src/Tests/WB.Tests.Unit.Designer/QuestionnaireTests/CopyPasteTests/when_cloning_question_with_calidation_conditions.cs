using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.CopyPasteTests
{
    [Subject(typeof(AbstractQuestion))]
    internal class when_cloning_question_with_validation_conditions
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            original = Create.TextQuestion(validationConditions: new List<ValidationCondition>
            {
                Create.ValidationCondition(message: "original")
            });
            clone = (TextQuestion)original.Clone();
            BecauseOf();
        }

        private void BecauseOf() => clone.ValidationConditions[0].Message = "changed";

        [NUnit.Framework.Test] public void should_not_change_original_validation_when_changing_title_in_clone () => original.ValidationConditions[0].Message.ShouldEqual("original");

        static TextQuestion original;
        static TextQuestion clone;
    }
}