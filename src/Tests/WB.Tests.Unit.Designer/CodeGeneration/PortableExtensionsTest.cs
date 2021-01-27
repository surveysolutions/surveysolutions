using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Unit.Designer.CodeGeneration
{
    public class PortableExtensionsTest
    {
        [TestCaseSource(nameof(TestCaseData))]
        public void split_string_test(CaseData caseData)
        {
            Assert.That(LookupHelper.SplitStringToLines(caseData.Data).ToArray(), 
                Is.EqualTo(caseData.Result));
        }

        public static CaseData[] TestCaseData =
        {
            new ("\r\n"),
            new ("1|2", "1|2"),
            new ("1\r\n", "1"),
            new ("\r\n2\r\n", "2"),
            new ("1\r\n2\r\n3", "1", "2", "3"),
            new ("1\n2\r\n3", "1", "2", "3"),
            new ("1\n2\n3", "1", "2", "3"),
            new ("1\n2\n3\n", "1", "2", "3"),
        };

        public record CaseData(string Data, params string[] Result);
    }
}
