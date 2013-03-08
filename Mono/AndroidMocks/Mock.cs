using System;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace AndroidMocks
{
	public class Mock<T> : RealProxy
	{
		private ICallHandler _callHandler;

		public Mock(ICallHandler callHandler): base(typeof(T))
		{
			_callHandler = callHandler;
		}

		public override IMessage Invoke(IMessage msg)
		{
			var mcm = (IMethodCallMessage)msg;
			try
			{
				var ret = _callHandler.Call(mcm.MethodName, mcm.Args);
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