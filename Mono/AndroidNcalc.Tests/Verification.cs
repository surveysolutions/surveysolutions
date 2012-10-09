using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
		
	}

	public interface IInterface
	{
		int ReturnInt();

		void DoNothing();
	}

	public class MyClass : IInterface
	{
		public int ReturnInt()
		{
			throw new NotImplementedException();
		}

		public void DoNothing()
		{
			throw new NotImplementedException();
		}
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