using System;
using System.Collections.Generic;
using System.Threading;
using WB.Common.Core.Logging;


namespace WB.Common
{
	public static class LogManager
	{
		private static readonly ReaderWriterLockSlim _cacheLocker = new ReaderWriterLockSlim();
		private static Dictionary<Type, ILog> _loggerCache = new Dictionary<Type, ILog>();

		public static ILog GetLogger(Type type)
		{
			ILog logger;
			_cacheLocker.EnterReadLock();

			try
			{
				if (_loggerCache.TryGetValue(type, out logger))
				{
					return logger;
				}
			}
			finally
			{
				_cacheLocker.ExitReadLock();
			}

			_cacheLocker.EnterWriteLock();
			try
			{
				// double check, as while the read-lock was released, the dictionary could have been modified
				if (_loggerCache.TryGetValue(type, out logger))
				{
					return logger;
				}

				logger = CreateLoggerForType(type);
				_loggerCache.Add(type, logger);
				return logger;
			}
			finally
			{
				_cacheLocker.ExitWriteLock();
			}
		}

		private static ILog CreateLoggerForType(Type type)
		{
			return new AndroidLogger();
		}
	}
}