using System;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
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
		public void ProxyTest()
		{
			var moq =(IInterface) (new Moq().GetTransparentProxy());

			Assert.That(moq.ReturnInt(), Is.EqualTo(7));
		}
	}

	public interface IInterface
	{
		int ReturnInt();
	}

	public class Moq : RealProxy
	{
		public Moq() : base(typeof(IInterface))
		{
		}

		public override IMessage Invoke(IMessage msg)
		{
			var mcm = (IMethodCallMessage)msg;
			try
			{
				//var ret = this.callHandler.Call(mcm.MethodName, mcm.Args);
				object ret = 7;
				if (ret == null)
				{
					var methodBase = mcm.MethodBase as MethodInfo;
					Type returnType = methodBase.ReturnType;
					if (returnType == typeof(bool))
					{
						ret = false;
					}
					if (returnType == typeof(byte))
					{
						ret = (byte)0;
					}
					if (returnType == typeof(sbyte))
					{
						ret = (sbyte)0;
					}
					if (returnType == typeof(decimal))
					{
						ret = 0M;
					}
					if (returnType == typeof(double))
					{
						ret = 0.0;
					}
					if (returnType == typeof(float))
					{
						ret = 0f;
					}
					if (returnType == typeof(int))
					{
						ret = 0;
					}
					if (returnType == typeof(uint))
					{
						ret = 0;
					}
					if (returnType == typeof(long))
					{
						ret = 0L;
					}
					if (returnType == typeof(ulong))
					{
						ret = (ulong)0L;
					}
					if (returnType == typeof(short))
					{
						ret = (short)0;
					}
					if (returnType == typeof(ushort))
					{
						ret = (ushort)0;
					}
					if (returnType == typeof(char))
					{
						ret = '?';
					}
				}
				return new ReturnMessage(ret, null, 0, null, mcm);
			}
			catch (System.Exception exception)
			{
				return new ReturnMessage(exception, mcm);
			}
		}
	}
}