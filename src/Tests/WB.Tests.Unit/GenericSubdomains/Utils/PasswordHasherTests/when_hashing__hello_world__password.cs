using Machine.Specifications;
using WB.Core.GenericSubdomains.Portable.Implementation;

namespace WB.Tests.Unit.GenericSubdomains.Utils.PasswordHasherTests
{
    internal class when_hashing__hello_world__password : PasswordHasherTestContext
    {
        Establish context = () =>
        {
            hasher = CreatePasswordHasher();
        };

        Because of = () => 
            result = hasher.Hash(password);

        It should = () =>
            result.ShouldEqual("45e08bb98ecaf19d96ca0046ae994ba7f83fa83d");

        private static PasswordHasher hasher;
        private static string password = "привет world";
        private static string result;
    }
}