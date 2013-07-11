using System;
using System.Collections.Generic;
using System.Threading;
using WB.Core.GenericSubdomains.Logging;


namespace WB.Core.SharedKernel.Utils.Logging
{
    public static class LogManager
    {
        private static readonly ReaderWriterLockSlim _cacheLocker = new ReaderWriterLockSlim();
        private static Dictionary<Type, ILogger> _loggerCache = new Dictionary<Type, ILogger>();

        public static ILogger GetLogger(Type type)
        {
            ILogger logger;
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

        private static ILogger CreateLoggerForType(Type type)
        {
            //return new AndroidLogger();
            return new FileLogger();
        }
    }
}