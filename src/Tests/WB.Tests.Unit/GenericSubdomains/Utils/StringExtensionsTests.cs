using System;
using CsQuery;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Utils;

namespace WB.Tests.Unit.GenericSubdomains.Utils
{
    [TestFixture]
    public class StringExtensionsTests
    {
        [Test]
        public void WhenCalledForMultiwordString()
        {
            Assert.That("SomePascalCased".ToCamelCase(), Is.EqualTo("somePascalCased"));
        }

        [Test]
        public void When_PascalCasing_CamelCased()
        {
            Assert.That("someCamelCase".ToPascalCase(), Is.EqualTo("SomeCamelCase"));
        }
    }


    [TestFixture]
    internal class MyTestFixture
    {
        private Calculator calculator;

        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            Console.WriteLine("TestFixureSetup");
        }

        [SetUp]
        public void Setup()
        {
            calculator = new Calculator();
            Console.WriteLine("Setup");
        }

        [Test]
        public void When_Adding_2_and_2_It_should_result_to_4()
        {
            // AAA

            // Arrange


            // Act 
            var result = calculator.Add(2, 2);
            

            // Assert
            Assert.That(result, Is.EqualTo(4));
        }

        [Test]
        public void When_Subtracting_2_and_2_It_should_result_to_0()
        {
            Console.WriteLine("Test2");
            var result = calculator.Subtract(2, 2);
            Assert.That(result, Is.EqualTo(0));
        }

        [TearDown]
        public void TearDown()
        {
            Console.WriteLine("Tear down");
            calculator.Dispose();
        }
    }

    internal class Calculator : IDisposable
    {
        public object Add(int i, int i1)
        {
            throw new System.NotImplementedException();
        }

        public object Subtract(int i, int i1)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}