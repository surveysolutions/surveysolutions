using System;

namespace AndroidMocks
{
	public class ExpectationException : Exception
	{
		public ExpectationException(): base("Not all expected methods were executed") {}
	}
}