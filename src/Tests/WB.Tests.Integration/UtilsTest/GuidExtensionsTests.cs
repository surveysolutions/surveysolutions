using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Helpers;

namespace WB.Tests.Integration.UtilsTest
{
    public class GuidExtensionsTests
    {
        [Test]
        public async Task when_rendering_guid_should_not_change_its_value()
        {
            Guid originalGuid = new Guid("DE6976CF-686E-4E5D-9369-CE6FC501DC0F");
            var compiledGuid = await CSharpScript.RunAsync<Guid>(originalGuid.AsBytesString());
            Assert.That(compiledGuid.ReturnValue, Is.EqualTo(originalGuid));
        }
    }
}