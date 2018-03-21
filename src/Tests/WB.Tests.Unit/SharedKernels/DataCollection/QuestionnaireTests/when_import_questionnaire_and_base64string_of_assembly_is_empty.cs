using System;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireTests
{
    internal class when_import_questionnaire_and_base64string_of_assembly_is_empty : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateImportedQuestionnaire(creatorId: responsibleId);
            BecauseOf();
        }

        public void BecauseOf() =>
            exception = Assert.Throws<QuestionnaireException>(
                () => questionnaire.ImportFromDesigner(Create.Command.ImportFromDesigner(responsibleId: responsibleId, base64StringOfAssembly: string.Empty)));

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__dont_have_permissions__ () =>
            new[] { "assembly", "empty" }.Should().OnlyContain(keyword => exception.Message.ToLower().Contains(keyword));

        private static Guid responsibleId = Guid.Parse("11111111111111111111111111111111");
        private static Guid unknownUserId = Guid.Parse("22222222222222222222222222222222");
        private static Questionnaire questionnaire;
        private static Exception exception;
    }
}
