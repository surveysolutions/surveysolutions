using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using AndroidMocks;
using FluentAssertions;
using Java.Lang;
using NCalc;
using NUnit.Framework;
using Math = System.Math;

namespace AndroidNcalc.Tests
{
	[TestFixture]
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

		[Test]
		public void FluentAssertionsTest()
		{
			var x = 3;
			x.Should().Be(3);
		}

		[Test]
		public void MockTests()
		{
			var mock = new DynamicMock<IInterface>();

			mock.Expect(x => x.ReturnInt(), 7);
			mock.Expect(x => x.DoNothing());

			var interfaceInstance = mock.Instance;

			interfaceInstance.DoNothing();

			Assert.That(interfaceInstance.ReturnInt(), Is.EqualTo(7));

			mock.VerifyAllExpectations();
		}

		[Test]
		public void DerivedTypesTest()
		{
			var type = typeof (DerivedClass);

			var method = type.GetMethods().First(m => m.Name == "PrintEntity");

			Assert.NotNull(method);

			Assert.That(method.ReflectedType, Is.EqualTo(typeof(DerivedClass)));

			var constuctor = method.ReflectedType.GetConstructor(new Type[0]);

			Assert.NotNull(constuctor);
		}
	}

	public interface IInterface
	{
		int ReturnInt();

		void DoNothing();
	}

	public abstract class AbstarctClass<T>
	{
		public abstract T GetEntity();

		public void PrintEntity()
		{
			var entity = GetEntity();

			//do some printing
		}
	}

	public class DerivedClass : AbstarctClass<int>
	{
		public override int GetEntity()
		{
			return 42;
		}
	}
}