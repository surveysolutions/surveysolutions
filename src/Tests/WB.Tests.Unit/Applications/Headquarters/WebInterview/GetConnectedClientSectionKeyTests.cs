using NUnit.Framework;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview
{
    public class GetConnectedClientSectionKeyTests
    {
        [TestCase("sectionA", "interviewA", ExpectedResult = "sectionAxinterviewA")]
        [TestCase(null, "interviewA", ExpectedResult = "PrefilledSectionxinterviewA", Description = "Should add prefilled marker if section null")]
        [TestCase("", "interviewA", ExpectedResult = "xinterviewA", Description = "Just a corner case test. Should not add prefilled")]
        public string ShouldGenerateKey(string section, string interview)
        {
            return UI.Headquarters.API.WebInterview.WebInterview.GetConnectedClientSectionKey(section, interview);
        }
    }
}