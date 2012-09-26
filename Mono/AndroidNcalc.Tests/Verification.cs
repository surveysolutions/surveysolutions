using System;
using Java.Lang;
using NCalc;
using NUnit.Framework;
using Math = System.Math;

namespace AndroidNcalc.Tests
{
	[TestFixture, Ignore]
	public class Verification
	{
		[Test]
		public void MathTest()
		{
			Assert.That(Math.Round(22.5,0, MidpointRounding.ToEven), Is.EqualTo(22));
			Assert.That(Math.Round(22.5,0, MidpointRounding.AwayFromZero), Is.EqualTo(22));
		}

		[Test]
		public void AssertTypeIdentification()
		{
			Assert.That(2.0f.GetType(), Is.EqualTo(typeof(float)));
		}
	}
}