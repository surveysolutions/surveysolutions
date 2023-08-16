using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.HttpServices.Services;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer
{
    [TestOf(typeof(KeywordsProvider))]
    internal class KeywordsProviderTests
    {
        //c# 
        [TestCase("abstract")]
        [TestCase("as")]
        [TestCase("base")]
        [TestCase("bool")]
        [TestCase("break")]
        [TestCase("byte")]
        [TestCase("case")]
        [TestCase("catch")]
        [TestCase("char")]
        [TestCase("checked")]
        [TestCase("class")]
        [TestCase("const")]
        [TestCase("continue")]
        [TestCase("decimal")]
        [TestCase("default")]
        [TestCase("delegate")]
        [TestCase("do")]
        [TestCase("double")]
        [TestCase("else")]
        [TestCase("enum")]
        [TestCase("event")]
        [TestCase("explicit")]
        [TestCase("extern")]
        [TestCase("false")]
        [TestCase("finally")]
        [TestCase("fixed")]
        [TestCase("float")]
        [TestCase("for")]
        [TestCase("foreach")]
        [TestCase("goto")]
        [TestCase("if")]
        [TestCase("implicit")]
        [TestCase("in")]
        [TestCase("int")]
        [TestCase("interface")]
        [TestCase("internal")]
        [TestCase("is")]
        [TestCase("lock")]
        [TestCase("long")]
        [TestCase("namespace")]
        [TestCase("new")]
        [TestCase("null")]
        [TestCase("object")]
        [TestCase("operator")]
        [TestCase("out")]
        [TestCase("override")]
        [TestCase("params")]
        [TestCase("private")]
        [TestCase("protected")]
        [TestCase("public")]
        [TestCase("readonly")]
        [TestCase("ref")]
        [TestCase("return")]
        [TestCase("sbyte")]
        [TestCase("sealed")]
        [TestCase("short")]
        [TestCase("sizeof")]
        [TestCase("stackalloc")]
        [TestCase("static")]
        [TestCase("string")]
        [TestCase("struct")]
        [TestCase("switch")]
        [TestCase("this")]
        [TestCase("throw")]
        [TestCase("true")]
        [TestCase("try")]
        [TestCase("typeof")]
        [TestCase("uint")]
        [TestCase("ulong")]
        [TestCase("unchecked")]
        [TestCase("unsafe")]
        [TestCase("ushort")]
        [TestCase("using")]
        [TestCase("virtual")]
        [TestCase("void")]
        [TestCase("volatile")]
        [TestCase("while")]
        //stata
        [TestCase("_all")]
        [TestCase("_b")]
        [TestCase("byte")]
        [TestCase("_coef")]
        [TestCase("_cons")]
        [TestCase("double")]
        [TestCase("float")]
        [TestCase("if")]
        [TestCase("in")]
        [TestCase("int")]
        [TestCase("long")]
        [TestCase("_n")]
        [TestCase("_pi")]
        [TestCase("_pred")]
        [TestCase("_rc")]
        [TestCase("_skip")]
        [TestCase("strl")]
        [TestCase("using")]
        [TestCase("with")]
        [TestCase("str1")]
        [TestCase("Str1")]
        [TestCase("str111")]
        [TestCase("str9999")]
        [TestCase("str1str")]
        [TestCase("str999a")]
        //spss
        [TestCase("all")]
        [TestCase("and")]
        [TestCase("by")]
        [TestCase("eq")]
        [TestCase("ge")]
        [TestCase("gt")]
        [TestCase("le")]
        [TestCase("lt")]
        [TestCase("ne")]
        [TestCase("not")]
        [TestCase("or")]
        [TestCase("to")]
        [TestCase("with")]
        //survey solutions
        [TestCase("rowcode")]
        [TestCase("rowname")]
        [TestCase("rowindex")]
        [TestCase("roster")]
        [TestCase("id")]
        [TestCase("parentid1")]
        [TestCase("parentid2")]
        [TestCase("parentid3")]
        [TestCase("parentid4")]
        [TestCase("self")]
        [TestCase("state")]
        [TestCase("quest")]
        [TestCase("questionnaire")]
        [TestCase("identity")]
        [TestCase("optioncode")]
        [TestCase("complete")]
        [TestCase("cover")]
        [TestCase("overview")]
        //windows reserved
        [TestCase("con")]
        [TestCase("prn")]
        [TestCase("aux")]
        [TestCase("nul")]
        [TestCase("com1")]
        [TestCase("com2")]
        [TestCase("com3")]
        [TestCase("com4")]
        [TestCase("com5")]
        [TestCase("com6")]
        [TestCase("com7")]
        [TestCase("com8")]
        [TestCase("com9")]
        [TestCase("lpt1")]
        [TestCase("lpt2")]
        [TestCase("lpt3")]
        [TestCase("lpt4")]
        [TestCase("lpt5")]
        [TestCase("lpt6")]
        [TestCase("lpt7")]
        [TestCase("lpt8")]
        [TestCase("lpt9")]
        public void should_keyword_be_reserved(string keyword)
        {
            //arrange
            var keywordProvider = CreateKeywordsProvider();
            //act
            var isKeywordReserved = keywordProvider.IsReservedKeyword(keyword);
            //assert
            Assert.IsTrue(isKeywordReserved);
        }

        [TestCase("myvar")]
        [TestCase("Str")]
        [TestCase("myvar_str1")]
        public void should_keyword_NOT_be_reserved(string keyword)
        {
            //arrange
            var keywordProvider = CreateKeywordsProvider();
            //act
            var isKeywordReserved = keywordProvider.IsReservedKeyword(keyword);
            //assert
            Assert.IsFalse(isKeywordReserved);
        }

        protected static IKeywordsProvider CreateKeywordsProvider(ISubstitutionService substitutionService = null)
        {
            return new KeywordsProvider(substitutionService ?? CreateSubstitutionService());
        }

        protected static ISubstitutionService CreateSubstitutionService()
        {
            return new SubstitutionService();
        }
    }
}
