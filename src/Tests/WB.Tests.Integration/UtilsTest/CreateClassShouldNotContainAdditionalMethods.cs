using System.Linq;
using System.Reflection;
using NUnit.Framework;
using WB.Tests.Abc;

namespace WB.Tests.Integration.UtilsTest
{
    [TestOf(typeof(Create))]
    public class CreateClassShouldNotContainAdditionalMethods
    {
        [Test]
        public void Should_contain_only_factory_methods_for_identity_and_roster_vector()
        {
            var factoryType = typeof(Create);

            var methodInfos = factoryType.GetMethods(BindingFlags.Static | BindingFlags.Public);

            Assert.That(methodInfos.Select(x => x.Name), Is.EquivalentTo(new [] {$"{nameof(Create.Identity)}", $"{nameof(Create.RosterVector)}" }), 
                "Create class should not be extended, add your helper factories in appropriate classes");
        }
    }
}