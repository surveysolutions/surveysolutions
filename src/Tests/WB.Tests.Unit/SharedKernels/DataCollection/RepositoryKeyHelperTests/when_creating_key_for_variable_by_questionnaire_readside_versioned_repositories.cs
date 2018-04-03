using FluentAssertions;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Tests.Unit.SharedKernels.DataCollection.RepositoryKeyHelperTests
{
    internal class when_creating_key_for_variable_by_questionnaire_readside_versioned_repositories
    {
        [NUnit.Framework.OneTimeSetUp] public void context () { BecauseOf();}

        public void BecauseOf() =>
            key = RepositoryKeysHelper.GetVariableByQuestionnaireKey("var", "11111111111111111111111111111111-1");

        [NUnit.Framework.Test] public void should_create_key_var_11111111111111111111111111111111_1 () =>
            key.Should().Be("var-11111111111111111111111111111111-1");

        private static string key;
    }
}
